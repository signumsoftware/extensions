﻿import * as React from 'react'
import * as QueryString from "query-string"
import { Lite } from '@framework/Signum.Entities'
import { parseLite } from '@framework/Signum.Entities'
import * as Navigator from '@framework/Navigator'
import { ChartRequest, UserChartEntity } from '../Signum.Entities.Chart'
import * as ChartClient from '../ChartClient'
import ChartRequestView from './ChartRequestView'
import { RouteComponentProps } from 'react-router'

interface ChartRequestPageProps extends RouteComponentProps<{ queryName: string; }> {

}

export default class ChartRequestPage extends React.Component<ChartRequestPageProps, { chartRequest?: ChartRequest; userChart?: Lite<UserChartEntity> }> {

  constructor(props: ChartRequestPageProps) {
    super(props);
    this.state = {};
  }

  componentWillMount() {
    this.load(this.props);
  }

  componentWillReceiveProps(nextProps: ChartRequestPageProps) {
    this.load(nextProps);
  }

  load(props: ChartRequestPageProps) {

    var oldPath = this.state.chartRequest && ChartClient.Encoder.chartPath(this.state.chartRequest, this.state.userChart);
    var newPath = props.location.pathname + props.location.search;

    if (oldPath != newPath) {
      var query = QueryString.parse(props.location.search);
      var uc = query.userChart == null ? undefined : (parseLite(query.userChart) as Lite<UserChartEntity>);
      ChartClient.Decoder.parseChartRequest(props.match.params.queryName, query)
        .then(cr => this.setState({ chartRequest: cr, userChart: uc }))
        .done();
    }
  }

  handleOnChange = (cr: ChartRequest, uc?: Lite<UserChartEntity>) => {
    var path = ChartClient.Encoder.chartPath(cr, uc);

    Navigator.history.replace(path);
  }

  render() {
    return <ChartRequestView
      chartRequest={this.state.chartRequest!}
      userChart={this.state.userChart}
      onChange={(cr, uc) => this.handleOnChange(cr, uc)} />;
  }
}


