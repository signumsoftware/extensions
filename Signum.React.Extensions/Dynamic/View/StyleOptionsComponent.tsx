import * as React from 'react'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { classes, Dic } from '@framework/Globals'
import { Binding } from '@framework/Reflection'
import { EntityControlMessage } from '@framework/Signum.Entities'
import { ExpressionOrValueComponent, DesignerModal } from './Designer'
import { DesignerNode } from './NodeUtils'
import { BaseNode } from './Nodes'
import { StyleOptionsExpression, formGroupStyle, formSize } from './StyleOptionsExpression'

interface StyleOptionsLineProps {
  binding: Binding<StyleOptionsExpression | undefined>;
  dn: DesignerNode<BaseNode>;
}

export class StyleOptionsLine extends React.Component<StyleOptionsLineProps>{

  renderMember(expr: StyleOptionsExpression | undefined): React.ReactNode {
    return (<span
      className={expr === undefined ? "design-default" : "design-changed"}>
      {this.props.binding.member}
    </span>);
  }

  handleRemove = (e: React.MouseEvent<any>) => {
    e.preventDefault();
    this.props.binding.deleteValue();
    this.props.dn.context.refreshView();
  }

  handleCreate = (e: React.MouseEvent<any>) => {
    e.preventDefault();
    this.modifyExpression({} as StyleOptionsExpression);
  }

  handleView = (e: React.MouseEvent<any>) => {
    e.preventDefault();
    var hae = JSON.parse(JSON.stringify(this.props.binding.getValue())) as StyleOptionsExpression;
    this.modifyExpression(hae);
  }

  modifyExpression(soe: StyleOptionsExpression) {

    DesignerModal.show("StyleOptions", () => <StyleOptionsComponent dn={this.props.dn} styleOptions={soe} />).then(result => {
      if (result) {

        if (Dic.getKeys(soe).length == 0)
          this.props.binding.deleteValue();
        else
          this.props.binding.setValue(soe);
      }

      this.props.dn.context.refreshView();
    }).done();
  }

  render() {
    const val = this.props.binding.getValue();

    return (
      <div className="form-group form-group-xs">
        <label className="control-label label-xs">
          {this.renderMember(val)}

          {val && " "}
          {val && <a href="#" className={classes("sf-line-button", "sf-remove")}
            onClick={this.handleRemove}
            title={EntityControlMessage.Remove.niceToString()}>
            <FontAwesomeIcon icon="times" />
          </a>}
        </label>
        <div>
          {val ?
            <a href="#" onClick={this.handleView}><pre style={{ padding: "0px", border: "none" }}>{this.getDescription(val)}</pre></a>
            :
            <a href="#" title={EntityControlMessage.Create.niceToString()}
              className="sf-line-button sf-create"
              onClick={this.handleCreate}>
              <FontAwesomeIcon icon="plus" className="sf-create" />&nbsp;{EntityControlMessage.Create.niceToString()}
            </a>}
        </div>
      </div>
    );
  }

  getDescription(soe: StyleOptionsExpression) {

    var keys = Dic.map(soe as any, (key, value) => key + ":" + value);
    return keys.join("\n");
  }
}

export interface StyleOptionsComponentProps {
  dn: DesignerNode<BaseNode>;
  styleOptions: StyleOptionsExpression
}

export class StyleOptionsComponent extends React.Component<StyleOptionsComponentProps>{
  render() {
    const so = this.props.styleOptions;
    const dn = this.props.dn;

    return (
      <div className="form-sm code-container">
        <ExpressionOrValueComponent dn={dn} refreshView={() => this.forceUpdate()} binding={Binding.create(so, s => s.formGroupStyle)} type="string" options={formGroupStyle} defaultValue={null} />
        <ExpressionOrValueComponent dn={dn} refreshView={() => this.forceUpdate()} binding={Binding.create(so, s => s.formSize)} type="string" options={formSize} defaultValue={null} />
        <ExpressionOrValueComponent dn={dn} refreshView={() => this.forceUpdate()} binding={Binding.create(so, s => s.placeholderLabels)} type="boolean" defaultValue={null} />
        <ExpressionOrValueComponent dn={dn} refreshView={() => this.forceUpdate()} binding={Binding.create(so, s => s.readonlyAsPlainText)} type="string" defaultValue={null} />
        <ExpressionOrValueComponent dn={dn} refreshView={() => this.forceUpdate()} binding={Binding.create(so, s => s.labelColumns)} type="number" defaultValue={null} />
        <ExpressionOrValueComponent dn={dn} refreshView={() => this.forceUpdate()} binding={Binding.create(so, s => s.valueColumns)} type="number" defaultValue={null} />
        <ExpressionOrValueComponent dn={dn} refreshView={() => this.forceUpdate()} binding={Binding.create(so, s => s.readOnly)} type="boolean" defaultValue={null} />
      </div>
    );
  }
}

