﻿import * as React from 'react'
import { ValueLine } from '@framework/Lines'
import { TypeContext } from '@framework/TypeContext'
import { TimeSpanEmbedded } from '../Signum.Entities.Basics'

export default class TimeSpan extends React.Component<{ ctx: TypeContext<TimeSpanEmbedded> }> {

  render() {

    const e = this.props.ctx;
    const sc = e.subCtx({ formGroupStyle: "BasicDown" });

    return (
      <div className="row">
        <div className="col-sm-3">
          <ValueLine ctx={sc.subCtx(n => n.days)} />
        </div>
        <div className="col-sm-3">
          <ValueLine ctx={sc.subCtx(n => n.hours)} />
        </div>
        <div className="col-sm-3">
          <ValueLine ctx={sc.subCtx(n => n.minutes)} />
        </div>
        <div className="col-sm-3">
          <ValueLine ctx={sc.subCtx(n => n.seconds)} />
        </div>
      </div>
    );
  }
}
