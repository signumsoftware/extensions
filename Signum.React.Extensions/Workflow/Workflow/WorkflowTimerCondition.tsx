﻿import * as React from 'react'
import { ValueLine, EntityLine, TypeContext, LiteAutocompleteConfig } from '@framework/Lines'
import { PropertyRoute } from '@framework/Reflection'
import CSharpCodeMirror from '../../Codemirror/CSharpCodeMirror'
import { WorkflowTimerConditionEntity } from '../Signum.Entities.Workflow'
import { API } from '../WorkflowClient'
import TypeHelpComponent from '../../TypeHelp/TypeHelpComponent'
import ValueLineModal from '@framework/ValueLineModal'

interface WorkflowTimerConditionComponentProps {
  ctx: TypeContext<WorkflowTimerConditionEntity>;
}

export default class WorkflowTimerConditionComponent extends React.Component<WorkflowTimerConditionComponentProps> {

  handleMainEntityTypeChange = () => {
    this.props.ctx.value.eval!.script = "";
    this.forceUpdate();
  }

  handleCodeChange = (newScript: string) => {
    const evalEntity = this.props.ctx.value.eval!;
    evalEntity.script = newScript;
    evalEntity.modified = true;
    this.forceUpdate();
  }

  render() {
    var ctx = this.props.ctx;

    return (
      <div>
        <ValueLine ctx={ctx.subCtx(wc => wc.name)} />
        <EntityLine ctx={ctx.subCtx(wc => wc.mainEntityType)}
          onChange={this.handleMainEntityTypeChange}
          autocomplete={new LiteAutocompleteConfig((ac, str) => API.findMainEntityType({ subString: str, count: 5 }, ac), false, false)}
          find={false} />
        {ctx.value.mainEntityType &&
          <div>
            <br />
            <div className="row">
              <div className="col-sm-7">
                <div className="code-container">
                  <pre style={{ border: "0px", margin: "0px" }}>{"boolean Evaluate(CaseActivityEntity ca, " + ctx.value.mainEntityType.cleanName + "Entity e, DateTime now)\n{"}</pre>
                  <CSharpCodeMirror script={ctx.value.eval!.script || ""} onChange={this.handleCodeChange} />
                  <pre style={{ border: "0px", margin: "0px" }}>{"}"}</pre>
                </div>
              </div>
              <div className="col-sm-5">
                <TypeHelpComponent initialType={ctx.value.mainEntityType.cleanName} mode="CSharp" onMemberClick={this.handleTypeHelpClick} />
              </div>
            </div>
          </div>}
      </div>
    );
  }

  handleTypeHelpClick = (pr: PropertyRoute | undefined) => {
    if (!pr)
      return;

    ValueLineModal.show({
      type: { name: "string" },
      initialValue: TypeHelpComponent.getExpression("e", pr, "CSharp"),
      valueLineType: "TextArea",
      title: "Property Template",
      message: "Copy to clipboard: Ctrl+C, ESC",
      initiallyFocused: true,
    }).done();
  }
}

