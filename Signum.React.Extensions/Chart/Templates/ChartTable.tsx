﻿import * as React from 'react'
import * as Finder from '@framework/Finder'
import * as Navigator from '@framework/Navigator'
import { ResultTable, ColumnOptionParsed, OrderOptionParsed, OrderType, ResultRow, hasAggregate, ColumnOption, FilterOptionParsed } from '@framework/FindOptions'
import { ChartRequest } from '../Signum.Entities.Chart'
import { toFilterOptions } from '@framework/Finder';

export default class ChartTable extends React.Component<{ resultTable: ResultTable; chartRequest: ChartRequest; lastChartRequest: ChartRequest; onRedraw: () => void }> {


  handleHeaderClick = (e: React.MouseEvent<any>) => {

    const tokenStr = (e.currentTarget as HTMLElement).getAttribute("data-column-name");

    const cr = this.props.chartRequest;

    const prev = cr.orderOptions.filter(a => a.token.fullKey == tokenStr).firstOrNull();

    if (prev != undefined) {
      prev.orderType = (prev.orderType == "Ascending" as OrderType) ? "Descending" : "Ascending";
      if (!e.shiftKey)
        cr.orderOptions = [prev];

    } else {

      const token = cr.columns.map(mle => mle.element.token!).filter(t => t && t.token!.fullKey == tokenStr).first("Column");

      const newOrder: OrderOptionParsed = { token: token.token!, orderType: "Ascending" };

      if (e.shiftKey)
        cr.orderOptions.push(newOrder);
      else
        cr.orderOptions = [newOrder];
    }

    this.props.onRedraw();
  }

  render() {

    const resultTable = this.props.resultTable;

    const chartRequest = this.props.chartRequest;

    const qs = Finder.getSettings(chartRequest.queryKey);

    const columns = chartRequest.columns.map(c => c.element).filter(cc => cc.token != undefined)
      .map(cc => ({ token: cc.token!.token, displayName: cc.displayName } as ColumnOptionParsed))
      .map(co => ({
        column: co,
        cellFormatter: (qs && qs.formatters && qs.formatters[co.token!.fullKey]) || Finder.formatRules.filter(a => a.isApplicable(co, undefined)).last("FormatRules").formatter(co),
        resultIndex: resultTable.columns.indexOf(co.token!.fullKey)
      }));


    const ctx: Finder.CellFormatterContext = {
      refresh: undefined
    }

    return (
      <table className="sf-search-results table table-hover table-sm">
        <thead>
          <tr>
            {!chartRequest.groupResults && <th></th>}
            {columns.map((col, i) =>
              <th key={i} data-column-name={col.column.token!.fullKey}
                onClick={this.handleHeaderClick}>
                <span className={"sf-header-sort " + this.orderClassName(col.column)} />
                <span> {col.column.displayName || col.column.token!.niceName}</span>
              </th>)}
          </tr>
        </thead>
        <tbody>
          {
            resultTable.rows.map((row, i) =>
              <tr key={i} onDoubleClick={e => this.handleOnDoubleClick(e, row)}>
                {!chartRequest.groupResults && <td>{((qs && qs.entityFormatter) || Finder.entityFormatRules.filter(a => a.isApplicable(row, undefined)).last("EntityFormatRules").formatter)(row, resultTable.columns, undefined)}</td>}
                {columns.map((c, j) =>
                  <td key={j} className={c.cellFormatter && c.cellFormatter.cellClass}>
                    {c.resultIndex == -1 || c.cellFormatter == undefined ? undefined : c.cellFormatter.formatter(row.columns[c.resultIndex], ctx)}
                  </td>)
                }
              </tr>
            )
          }
        </tbody>
      </table>

    );
  }

  handleOnDoubleClick = (e: React.MouseEvent<HTMLTableRowElement>, row: ResultRow) => {


    const lcr = this.props.lastChartRequest!;

    if (lcr.groupResults == false) {

      window.open(Navigator.navigateRoute(row.entity!));

    } else {

      const filters = lcr.filterOptions.filter(a => !hasAggregate(a.token));
      const columns: ColumnOption[] = [];

      lcr.columns.filter(a => a.element.token).map((a, i) => {

        const t = a.element.token!.token!;

        if (!hasAggregate(t)) {
          filters.push({
            token: t,
            operation: "EqualTo",
            value: row.columns[i],
            frozen: false
          } as FilterOptionParsed);
        }

        if (t.parent != undefined) //Avoid Count and simple Columns that are already added
        {
          var col = t.queryTokenType == "Aggregate" ? t.parent : t

          if (col.parent)
            columns.push({
              token: col.fullKey
            });
        }
      });

      window.open(Finder.findOptionsPath({
        queryName: lcr.queryKey,
        filterOptions: toFilterOptions(filters),
        columnOptions: columns,
      }));
    }
  }

  orderClassName(column: ColumnOptionParsed) {

    if (column.token == undefined)
      return "";

    const orders = this.props.chartRequest.orderOptions;

    const o = orders.filter(a => a.token.fullKey == column.token!.fullKey).firstOrNull();
    if (o == undefined)
      return "";

    let asc = (o.orderType == "Ascending" as OrderType) ? "asc" : "desc";

    if (orders.indexOf(o))
      asc += " l" + orders.indexOf(o);

    return asc;
  }
}




