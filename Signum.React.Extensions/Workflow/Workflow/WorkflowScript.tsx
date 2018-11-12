﻿import * as React from 'react'
import { ValueLine, EntityLine, TypeContext } from '@framework/Lines'
import CSharpCodeMirror from '../../Codemirror/CSharpCodeMirror'
import { WorkflowScriptEntity } from '../Signum.Entities.Workflow'
import TypeHelpComponent from '../../TypeHelp/TypeHelpComponent'
import ValueLineModal from '@framework/ValueLineModal'

interface WorkflowScriptComponentProps {
  ctx: TypeContext<WorkflowScriptEntity>;
}

export default class WorkflowScriptComponent extends React.Component<WorkflowScriptComponentProps> {

  handleMainEntityTypeChange = () => {
    this.props.ctx.value.eval!.script = "";
    this.forceUpdate();
  }

  handleScriptChange = (newScript: string) => {
    const evalEntity = this.props.ctx.value.eval!;
    evalEntity.script = newScript;
    evalEntity.modified = true;
    this.forceUpdate();
  }

  handleCustomTypesChange = (newScript: string) => {
    const scriptEval = this.props.ctx.value.eval!;
    scriptEval.customTypes = newScript;
    scriptEval.modified = true;
    this.forceUpdate();
  }

  render() {
    var ctx = this.props.ctx;

    return (
      <div>
        <ValueLine ctx={ctx.subCtx(ws => ws.name)} />
        <EntityLine ctx={ctx.subCtx(ws => ws.mainEntityType)} onChange={this.handleMainEntityTypeChange} />

        {ctx.value.mainEntityType &&
          <div>
            <br />
            <div className="row">
              <div className="col-sm-7">
                <div className="btn-group" style={{ marginBottom: "3px" }}>
                  <input type="button" className="btn btn-danger btn-sm sf-button" value="try-catch" onClick={this.handleTryCatchClick} />
                  <input type="button" className="btn btn-success btn-sm sf-button" value="REST" onClick={this.handleRestClick} />
                  <input type="button" className="btn btn-warning btn-sm sf-button" value="SOAP" onClick={this.handleSoapClick} />
                  <input type="button" className="btn btn-danger btn-sm sf-button" value="ctx" onClick={this.handleCtxClick} />
                </div>
                <div className="code-container">
                  <pre style={{ border: "0px", margin: "0px" }}>{`public static void ScriptCode(${ctx.value.mainEntityType.cleanName}Entity e, WorkflowScriptContext ctx)\n{`}</pre>
                  <CSharpCodeMirror script={ctx.value.eval!.script || ""} onChange={this.handleScriptChange} />
                  <pre style={{ border: "0px", margin: "0px" }}>{"}"}</pre>
                </div>
                <div className="code-container">
                  <pre style={{ border: "0px", margin: "0px" }}>{`namespace MyCustomTypes {`}</pre>
                  <CSharpCodeMirror script={ctx.value.eval!.customTypes || ""} onChange={this.handleCustomTypesChange} />
                  <pre style={{ border: "0px", margin: "0px" }}>{"}"}</pre>
                </div>
              </div>
              <div className="col-sm-5">
                <TypeHelpComponent initialType={ctx.value.mainEntityType.cleanName} mode="CSharp" />
              </div>
            </div>
          </div>}
      </div>
    );
  }

  handleRestClick = () => {
    ValueLineModal.show({
      type: { name: "string" },
      initialValue: `// REST
var response = HttpClient.Post<MyResponse>("Your URL", new { paramName = e.[Property Name], ... });
e.[Property Name] = response.[Property Name];

class MyResponse {}`,
      valueLineType: "TextArea",
      title: "REST Template",
      message: "Copy to clipboard: Ctrl+C, ESC",
      initiallyFocused: true,
      valueHtmlAttributes: { style: { height: "115px" } },
    }).done();
  }

  handleSoapClick = () => {
    ValueLineModal.show({
      type: { name: "string" },
      initialValue: `// SOAP
var lib = Assembly.Load("[Assembly full path name]").GetType("[Type Name]").GetMethod("[Method Name]").Invoke(e.[Property Name]);
e.[Property Name] = lib;`,
      valueLineType: "TextArea",
      title: "SOAP Template",
      message: "Copy to clipboard: Ctrl+C, ESC",
      initiallyFocused: true,
      valueHtmlAttributes: { style: { height: "115px" } },
    }).done();
  }

  handleCtxClick = () => {
    const hint = "WorkflowScriptContext Members";
    ValueLineModal.show({
      type: { name: "string" },
      initialValue: `// ${hint}
CaseActivityEntity CaseActivity; 
int RetryCount;`,
      valueLineType: "TextArea",
      title: hint,
      message: "Copy to clipboard: Ctrl+C, ESC",
      initiallyFocused: true,
      valueHtmlAttributes: { style: { height: "115px" } },
    }).done();
  }

  handleTryCatchClick = () => {
    ValueLineModal.show({
      type: { name: "string" },
      initialValue: `try
{

}
catch (Exception e)
{
    throw e;
}`,
      valueLineType: "TextArea",
      title: "Try/Catch block",
      message: "Copy to clipboard: Ctrl+C, ESC",
      initiallyFocused: true,
      valueHtmlAttributes: { style: { height: "180px" } },
    }).done();
  }
}

