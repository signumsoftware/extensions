import * as React from 'react'
import * as d3 from 'd3'
import * as ChartClient from '../ChartClient';
import * as ChartUtils from './Components/ChartUtils';
import { translate, scale, rotate, skewX, skewY, matrix, scaleFor } from './Components/ChartUtils';
import { PivotRow, toPivotTable, groupedPivotTable } from './Components/PivotTable';
import { ChartTable, ChartColumn } from '../ChartClient';
import { XKeyTicks, YScaleTicks, XTitle } from './Components/Ticks';
import Legend from './Components/Legend';
import { XAxis, YAxis } from './Components/Axis';
import TextEllipsis from './Components/TextEllipsis';
import { Rule } from './Components/Rule';
import InitialMessage from './Components/InitialMessage';


export default function renderStackedColumns({ data, width, height, parameters, loading, onDrillDown, initialLoad, chartRequest }: ChartClient.ChartScriptProps): React.ReactElement<any> {

  var xRule = Rule.create({
    _1: 5,
    title: 15,
    _2: 10,
    labels: parseInt(parameters["UnitMargin"]),
    _3: 5,
    ticks: 4,
    content: '*',
    _4: 10,
  }, width);
  //xRule.debugX(chart)

  var yRule = Rule.create({
    _1: 10,
    legend: 15,
    _2: 5,
    content: '*',
    ticks: 4,
    _3: parameters["Labels"] == "Margin" ? 5 : 0,
    labels: parameters["Labels"] == "Margin" ? parseInt(parameters["LabelsMargin"]) : 0,
    _4: 10,
    title: 15,
    _5: 5,
  }, height);
  //yRule.debugY(chart);

  if (data == null || data.rows.length == 0)
    return (
      <svg direction="ltr" width={width} height={height}>
        <InitialMessage data={data} x={xRule.middle("content")} y={yRule.middle("content")} loading={loading} />
        <XAxis xRule={xRule} yRule={yRule} />
        <YAxis xRule={xRule} yRule={yRule} />
      </svg>
    );

  var c = data.columns;
  var keyColumn = c.c0 as ChartColumn<unknown>;
  var valueColumn0 = c.c2 as ChartColumn<number>;

  var pivot = c.c1 == null ?
    toPivotTable(data, c.c0!, [c.c2, c.c3, c.c4, c.c5, c.c6].filter(cn => cn != undefined) as ChartColumn<number>[]) :
    groupedPivotTable(data, c.c0!, c.c1, c.c2 as ChartColumn<number>);


  var keyValues = ChartUtils.completeValues(keyColumn, pivot.rows.map(r => r.rowValue), parameters['CompleteValues'], chartRequest.filterOptions, ChartUtils.insertPoint(keyColumn, valueColumn0));

  var x = d3.scaleBand()
    .domain(keyValues.map(v => keyColumn.getKey(v)))
    .range([0, xRule.size('content')]);

  var pStack = parameters["Stack"];

  var stack = d3.stack<PivotRow>()
    .offset(ChartUtils.getStackOffset(pStack)!)
    .order(ChartUtils.getStackOrder(parameters["Order"])!)
    .keys(pivot.columns.map(d => d.key))
    .value((r, k) => r.values[k]?.value ?? 0);

  var stackedSeries = stack(pivot.rows);

  var max = d3.max(stackedSeries, s => d3.max(s, vs => vs[1]))!;
  var min = d3.min(stackedSeries, s => d3.min(s, vs => vs[0]))!;

  var y = d3.scaleLinear()
    .domain([min, max])
    .range([0, yRule.size('content')]);

  var rowsInOrder = pivot.rows.orderBy(r => keyColumn.getKey(r.rowValue));
  var color = ChartUtils.colorCategory(parameters, pivot.columns.map(c => c.key));
  var colorByKey = pivot.columns.toObject(a => a.key, a => a.color);

  var format = pStack == "expand" ? d3.format(".0%") :
    pStack == "zero" ? d3.format("") :
      (n: number) => d3.format("")(n) + "?";

  var labelMargin = 10;
  var size = yRule.size('content');

  return (
    <svg direction="ltr" width={width} height={height}>
      <XTitle xRule={xRule} yRule={yRule} keyColumn={keyColumn} />
      <YScaleTicks xRule={xRule} yRule={yRule} valueColumn={valueColumn0} y={y} format={format} />

      {stackedSeries.orderBy(s => s.key).map(s => <g key={s.key} className="shape-serie"
        transform={translate(xRule.start('content'), yRule.end('content'))} >

        {s.orderBy(r => keyColumn.getKey(r.data.rowValue))
          .filter(r => r.data.values[s.key] != undefined)
          .map(r => <rect key={keyColumn.getKey(r.data.rowValue)} className="shape sf-transition"
            transform={translate(x(keyColumn.getKey(r.data.rowValue))!, -y(r[1])!) + (initialLoad ? scale(1, 0) : scale(1, 1))}
            stroke={x.bandwidth() > 4 ? '#fff' : undefined}
            fill={colorByKey[s.key] ?? color(s.key)}
            width={x.bandwidth()}
            height={y(r[1])! - y(r[0])!}
            onClick={e => onDrillDown(r.data.values[s.key].rowClick, e)}
            cursor="pointer">
            <title>
              {r.data.values[s.key].valueTitle}
            </title>
          </rect>)}

        {parseFloat(parameters["NumberOpacity"]) > 0 && x.bandwidth() > 15 &&
          s.orderBy(r => keyColumn.getKey(r.data.rowValue))
            .filter(r => (y(r[1])! - y(r[0])!) > 10)
            .map(r => <text key={keyColumn.getKey(r.data.rowValue)} className="number-label sf-transition"
              transform={translate(
                x(keyColumn.getKey(r.data.rowValue))! + x.bandwidth() / 2,
                -y(r[0])! * 0.5 - y(r[1])! * 0.5
              )}
              onClick={e => onDrillDown(r.data.values[s.key].rowClick, e)}
              fill={parameters["NumberColor"]}
              dominantBaseline="middle"
              opacity={parameters["NumberOpacity"]}
              textAnchor="middle"
              fontWeight="bold">
              {r.data.values[s.key].valueNiceName}
              <title>
                {r.data.values[s.key].valueTitle}
              </title>
            </text>)
        }
      </g>)}

      {x.bandwidth() > 15 && (
        parameters["Labels"] == "Margin" ?
          <g className="x-label" transform={translate(xRule.start('content'), yRule.start('labels'))}>
            {keyValues.map(k => <TextEllipsis key={keyColumn.getKey(k)}
              maxWidth={yRule.size('labels')}
              padding={labelMargin}
              className="x-label sf-transition"
              transform={translate(x(keyColumn.getKey(k))! + x.bandwidth() / 2, 0) + rotate(-90)}
              dominantBaseline="middle"
              fill="black"
              shapeRendering="geometricPrecision"
              textAnchor="end">
              {keyColumn.getNiceName(k)}
            </TextEllipsis>)}
          </g> :
          parameters["Labels"] == "Inside" ?
            <g className="x-label" transform={translate(xRule.start('content'), yRule.end('content'))}>
              {keyValues.map((k, i) => {
                var maxValue = stackedSeries[stackedSeries.length - 1][i][1];
                var posy = y(maxValue)!;

                return (<TextEllipsis key={keyColumn.getKey(k)}
                  maxWidth={posy >= size / 2 ? posy : size - posy}
                  padding={labelMargin}
                  className="x-label sf-transition"
                  transform={translate(x(keyColumn.getKey(k))! + x.bandwidth() / 2, posy >= size / 2 ? 0 : -posy) + rotate(-90)}
                  dominantBaseline="middle"
                  fontWeight="bold"
                  fill={posy >= size / 2 ? '#fff' : '#000'}
                  dx={labelMargin}
                  textAnchor="start">
                  {keyColumn.getNiceName(k)}
                </TextEllipsis>);
              })}
            </g> : undefined
      )}

      <Legend pivot={pivot} xRule={xRule} yRule={yRule} color={color} />

      <InitialMessage data={data} x={xRule.middle("content")} y={yRule.middle("content")} loading={loading} />
      <XAxis xRule={xRule} yRule={yRule} />
      <YAxis xRule={xRule} yRule={yRule} />
    </svg>
  );
}
