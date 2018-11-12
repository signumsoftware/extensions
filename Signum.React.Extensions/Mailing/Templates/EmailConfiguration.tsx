﻿import * as React from 'react'
import { ValueLine, EntityCombo } from '@framework/Lines'
import { TypeContext } from '@framework/TypeContext'
import { EmailConfigurationEmbedded } from '../Signum.Entities.Mailing'

export default class EmailConfiguration extends React.Component<{ ctx: TypeContext<EmailConfigurationEmbedded> }> {
  render() {
    const sc = this.props.ctx;
    const ac = this.props.ctx.subCtx({ formGroupStyle: "Basic" });

    return (
      <div>
        <ValueLine ctx={sc.subCtx(ca => ca.reciveEmails)} />
        <ValueLine ctx={sc.subCtx(ca => ca.sendEmails)} />
        <ValueLine ctx={sc.subCtx(ca => ca.overrideEmailAddress)} />
        <EntityCombo ctx={sc.subCtx(ca => ca.defaultCulture)} />
        <ValueLine ctx={sc.subCtx(ca => ca.urlLeft)} />

        <fieldset>
          <legend>Async</legend>
          <div className="row">
            <div className="col-sm-6">
              <ValueLine ctx={ac.subCtx(ca => ca.avoidSendingEmailsOlderThan)} />
              <ValueLine ctx={ac.subCtx(ca => ca.chunkSizeSendingEmails)} />
            </div>
            <div className="col-sm-6">
              <ValueLine ctx={ac.subCtx(ca => ca.maxEmailSendRetries)} />
              <ValueLine ctx={ac.subCtx(ca => ca.asyncSenderPeriod)} />
            </div>
          </div>
        </fieldset>
      </div>
    );
  };
}

