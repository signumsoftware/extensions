
import * as React from 'react'
import { Dic } from '@framework/Globals';
import { openModal, IModalProps } from '@framework/Modals';
import * as NodeUtils from './NodeUtils'
import { BaseNode } from './Nodes'
import { Modal } from '@framework/Components';

interface ShowCodeModalProps extends React.Props<ShowCodeModal>, IModalProps {
  typeName: string;
  node: BaseNode;
}

export default class ShowCodeModal extends React.Component<ShowCodeModalProps, { show: boolean }>  {

  constructor(props: ShowCodeModalProps) {
    super(props);

    this.state = { show: true };
  }

  handleCancelClicked = () => {
    this.setState({ show: false });
  }

  handleOnExited = () => {
    this.props.onExited!(undefined);
  }

  render() {
    return (
      <Modal size="lg" onHide={this.handleCancelClicked} show={this.state.show} onExited={this.handleOnExited} className="sf-selector-modal">
        <div className="modal-header">
          <h5 className="modal-title">{this.props.typeName + "Component code"}</h5>
          <button type="button" className="close" data-dismiss="modal" aria-label="Close" onClick={this.handleCancelClicked}>
            <span aria-hidden="true">&times;</span>
          </button>
        </div>
        <div className="modal-body">
          <pre>
            {renderFile(this.props.typeName, this.props.node)}
          </pre>
        </div>
      </Modal>
    );
  }

  static showCode(typeName: string, node: BaseNode): Promise<void> {
    return openModal<void>(<ShowCodeModal typeName={typeName} node={node} />);
  }
}


function renderFile(typeName: string, node: BaseNode): string {

  var cc = new NodeUtils.CodeContext("ctx", [], {}, []);

  var text = NodeUtils.renderCode(node, cc).indent(12);

  return (
    `
import * as React from 'react'
import { Dic } from '@framework/Globals'
import { getMixin } from '@framework/Signum.Entities'
import { ${typeName}Entity } from '../[your namespace]'
import { ValueLine, EntityLine, RenderEntity, EntityCombo, EntityList, EntityDetail, EntityStrip, 
         EntityRepeater, EntityCheckboxList, EntityTabRepeater, TypeContext, EntityTable } from '@framework/Lines'
import { SearchControl, ValueSearchControl } from '@framework/Search'
${Dic.getValues(cc.imports.toObjectDistinct(a => a)).join("\n")}

export default class ${typeName}Component extends React.Component<{ ctx: TypeContext<${typeName}Entity> }> {

    render() {
        const ctx = this.props.ctx;
${Dic.map(cc.assignments, (k, v) => `const ${k} = ${v};`).join("\n").indent(8)}
        return (
${text}
        );
    }
}`
  );

}





