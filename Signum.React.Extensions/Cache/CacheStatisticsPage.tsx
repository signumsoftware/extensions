﻿
import * as React from 'react'
import { RouteComponentProps } from 'react-router-dom'
import { API, CacheTableStats, ResetLazyStats } from './CacheClient'
import { Tab, UncontrolledTabs } from '@framework/Components/Tabs'

interface CacheStatisticsPageProps extends RouteComponentProps<{}> {

}

interface CacheStatisticsPageState {
  isEnabled?: boolean;
  tables?: CacheTableStats[];
  lazies?: ResetLazyStats[];
}

export default class CacheStatisticsPage extends React.Component<CacheStatisticsPageProps, CacheStatisticsPageState> {

  constructor(props: CacheStatisticsPageProps) {
    super(props);

    this.state = {};
  }

  componentWillMount() {
    this.loadState().done();
  }

  loadState() {
    return API.view()
      .then(s => this.setState({
        tables: s.tables,
        lazies: s.lazies,
        isEnabled: s.isEnabled
      }));
  }

  handleDisabled = (e: React.MouseEvent<any>) => {
    API.disable().then(() => this.loadState()).done();
  }

  handleEnabled = (e: React.MouseEvent<any>) => {
    API.enable().then(() => this.loadState()).done();
  }

  handleClear = (e: React.MouseEvent<any>) => {
    API.clear().then(() => this.loadState()).done();
  }

  render() {

    return (
      <div>
        <h2>Cache Statistics</h2>
        <div className="btn-toolbar">
          {this.state.isEnabled == true && <button onClick={this.handleDisabled} className="sf-button btn btn-light" style={{ color: "red" }}>Disable</button>}
          {this.state.isEnabled == false && <button onClick={this.handleEnabled} className="sf-button btn btn-light" style={{ color: "green" }}>Enabled</button>}
          {<button onClick={this.handleClear} className="sf-button btn btn-light" style={{ color: "blue" }}>Clear</button>}
        </div >
        <UncontrolledTabs id="tabs">
          {this.state.tables &&
            <Tab title="Tables" eventKey="table">
              {this.renderTables()}
            </Tab>}
          {this.state.lazies &&
            <Tab title="Lazies" eventKey="lazy">
              {this.renderLazies()}
            </Tab>
          }
        </UncontrolledTabs>


      </div>
    );
  }

  renderLazies() {
    return (
      <table className="table table-sm">
        <thead>
          <tr>
            <th>Type</th>
            <th>Hits</th>
            <th>Invalidations</th>
            <th>Loads</th>
            <th>LoadTime</th>
          </tr>
        </thead>
        <tbody>
          {this.state.lazies!.map((lazy, i) => <tr key={i}>
            <td> {lazy.typeName} </td>
            <td> {lazy.hits} </td>
            <td> {lazy.invalidations}</td>
            <td> {lazy.loads}</td>
            <td> {lazy.sumLoadTime} </td>
          </tr>)}
        </tbody>
      </table>);
  }

  renderTables() {

    const list: React.ReactNode[] = [];
    if (this.state.tables)
      this.state.tables.forEach(st => this.showTree(list, st, 0));

    return (
      <table className="table table-sm">
        <thead>
          <tr>
            <th>Table</th>
            <th>Type</th>
            <th>Count</th>
            <th>Hits</th>
            <th>Invalidations</th>
            <th>Loads</th>
            <th>LoadTime</th>
          </tr>
        </thead>
        <tbody>
          {list}
        </tbody>
      </table>);
  }


  showTree(list: React.ReactNode[], table: CacheTableStats, depth: number) {

    const opacity =
      depth == 0 ? 1 :
        depth == 1 ? .7 :
          depth == 2 ? .5 :
            depth == 3 ? .4 : .3;

    list.push(
      <tr style={{ opacity: opacity }} key={list.length}>
        <td> {Array.repeat(depth, " → ").join("") + table.tableName}</td >
        <td> {table.typeName} </td>
        <td> {table.count != undefined ? table.count.toString() : "-- not loaded --"} </td>
        <td> {table.hits} </td>
        <td> {table.invalidations}</td>
        <td> {table.loads}</td>
        <td> {table.sumLoadTime} </td>
      </tr>
    );

    if (table.subTables)
      table.subTables.forEach(st => this.showTree(list, st, depth + 1));
  }
}



