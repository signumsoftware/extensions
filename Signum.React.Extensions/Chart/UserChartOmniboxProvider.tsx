﻿import * as React from 'react'
import { Lite } from '@framework/Signum.Entities'
import { OmniboxResult, OmniboxMatch, OmniboxProvider } from '../Omnibox/OmniboxClient'
import * as Navigator from '@framework/Navigator'
import * as ChartClient from './ChartClient'
import * as UserChartClient from './UserChart/UserChartClient'
import { UserChartEntity } from './Signum.Entities.Chart'

export default class UserChartOmniboxProvider extends OmniboxProvider<UserChartOmniboxResult>
{
  getProviderName() {
    return "UserChartOmniboxResult";
  }

  icon() {
    return this.coloredIcon("chart-bar", "darkviolet");
  }

  renderItem(result: UserChartOmniboxResult): React.ReactChild[] {

    const array: React.ReactChild[] = [];

    array.push(this.icon());

    this.renderMatch(result.toStrMatch, array);

    return array;
  }

  navigateTo(result: UserChartOmniboxResult) {

    if (result.userChart == undefined)
      return undefined;

    return Navigator.API.fetchAndForget(result.userChart)
      .then(a => UserChartClient.Converter.toChartRequest(a, undefined))
      .then(cr => ChartClient.Encoder.chartPath(cr, result.userChart));
  }

  toString(result: UserChartOmniboxResult) {
    return "\"{0}\"".formatWith(result.toStrMatch.text);
  }
}

interface UserChartOmniboxResult extends OmniboxResult {
  toStr: string;
  toStrMatch: OmniboxMatch;

  userChart: Lite<UserChartEntity>;
}
