﻿import * as React from 'react'
import { notifySuccess } from '@framework/Operations/EntityOperations'
import { TypeContext, ButtonsContext, IRenderButtons } from '@framework/TypeContext'
import { EntityLine, ValueLine } from '@framework/Lines'
import { OperationSymbol } from '@framework/Signum.Entities'
import { API } from '../AuthClient'
import { OperationRulePack, OperationAllowed, OperationAllowedRule, AuthAdminMessage, PermissionSymbol, AuthMessage } from '../Signum.Entities.Authorization'
import { ColorRadio, GrayCheckbox } from './ColoredRadios'
import { Button } from '@framework/Components';
import "./AuthAdmin.css"

export default class OperationRulePackControl extends React.Component<{ ctx: TypeContext<OperationRulePack> }> implements IRenderButtons {

  handleSaveClick = (bc: ButtonsContext) => {
    let pack = this.props.ctx.value;

    API.saveOperationRulePack(pack)
      .then(() => API.fetchOperationRulePack(pack.type.cleanName!, pack.role.id!))
      .then(newPack => {
        notifySuccess();
        bc.frame.onReload({ entity: newPack, canExecute: {} });
      })
      .done();
  }

  renderButtons(bc: ButtonsContext) {
    return [
      <Button color="primary" onClick={() => this.handleSaveClick(bc)}>{AuthMessage.Save.niceToString()}</Button>
    ];
  }

  handleHeaderClick(e: React.MouseEvent<HTMLAnchorElement>, hc: OperationAllowed) {

    this.props.ctx.value.rules.forEach(mle => {
      if (!mle.element.coercedValues!.contains(hc)) {
        mle.element.allowed = hc;
        mle.element.modified = true;
      }
    });

    this.forceUpdate();
  }

  render() {

    let ctx = this.props.ctx;

    return (
      <div>
        <div className="form-compact">
          <EntityLine ctx={ctx.subCtx(f => f.role)} />
          <ValueLine ctx={ctx.subCtx(f => f.strategy)} />
          <EntityLine ctx={ctx.subCtx(f => f.type)} />
        </div>
        <table className="table table-sm sf-auth-rules">
          <thead>
            <tr>
              <th>
                {OperationSymbol.niceName()}
              </th>
              <th style={{ textAlign: "center" }}>
                <a onClick={e => this.handleHeaderClick(e, "Allow")}>{OperationAllowed.niceToString("Allow")}</a>
              </th>
              <th style={{ textAlign: "center" }}>
                <a onClick={e => this.handleHeaderClick(e, "DBOnly")}>{OperationAllowed.niceToString("DBOnly")}</a>
              </th>
              <th style={{ textAlign: "center" }}>
                <a onClick={e => this.handleHeaderClick(e, "None")}>{OperationAllowed.niceToString("None")}</a>
              </th>
              <th style={{ textAlign: "center" }}>
                {AuthAdminMessage.Overriden.niceToString()}
              </th>
            </tr>
          </thead>
          <tbody>
            {ctx.mlistItemCtxs(a => a.rules).map((c, i) =>
              <tr key={i}>
                <td>
                  {c.value.resource!.operation!.toStr}
                </td>
                <td style={{ textAlign: "center" }}>
                  {this.renderRadio(c.value, "Allow", "green")}
                </td>
                <td style={{ textAlign: "center" }}>
                  {this.renderRadio(c.value, "DBOnly", "#FFAD00")}
                </td>
                <td style={{ textAlign: "center" }}>
                  {this.renderRadio(c.value, "None", "red")}
                </td>
                <td style={{ textAlign: "center" }}>
                  <GrayCheckbox checked={c.value.allowed != c.value.allowedBase} onUnchecked={() => {
                    c.value.allowed = c.value.allowedBase;
                    ctx.value.modified = true;
                    this.forceUpdate();
                  }} />
                </td>
              </tr>
            )
            }
          </tbody>
        </table>

      </div>
    );
  }

  renderRadio(c: OperationAllowedRule, allowed: OperationAllowed, color: string) {

    if (c.coercedValues!.contains(allowed))
      return;

    return <ColorRadio checked={c.allowed == allowed} color={color} onClicked={a => { c.allowed = allowed; c.modified = true; this.forceUpdate() }} />;
  }
}



