﻿import * as React from 'react'
import { SubTokensOptions, QueryToken, hasAnyOrAll } from '@framework/FindOptions'
import { TemplateTokenMessage } from './Signum.Entities.Templating'
import QueryTokenBuilder from '@framework/SearchControl/QueryTokenBuilder'

export interface TemplateControlsProps {
  queryKey: string;
  onInsert: (newCode: string) => void;
  forHtml: boolean
}

export interface TemplateControlsState {
  currentToken: QueryToken | undefined
}

export default class TemplateControls extends React.Component<TemplateControlsProps, TemplateControlsState>{

  constructor(props: TemplateControlsProps) {
    super(props);
    this.state = { currentToken: undefined } as TemplateControlsState;
  }

  render() {
    const ct = this.state.currentToken;

    if (!this.props.queryKey)
      return null;

    return (
      <div>
        <span className="rw-widget-sm">
          <QueryTokenBuilder queryToken={ct} queryKey={this.props.queryKey} onTokenChange={t => this.setState({ currentToken: t || undefined })} subTokenOptions={SubTokensOptions.CanAnyAll | SubTokensOptions.CanElement} readOnly={false} />
        </span>
        <div className="btn-group" style={{ marginLeft: "10px" }}>
          {this.renderButton(TemplateTokenMessage.Insert.niceToString(), this.canElement(), token => `@[${token}]`)}
          {this.renderButton("if", this.canIf(), token => this.props.forHtml ?
            `<!--@if[${token}]--> <!--@else--> <!--@endif-->` :
            `@if[${token}] @else @endif`)}
          {this.renderButton("foreach", this.canForeach(), token => this.props.forHtml ?
            `<!--@foreach[${token}]--> <!--@endforeach-->` :
            `@foreach[${token}] @endforeach`)}
          {this.renderButton("any", this.canElement(), token => this.props.forHtml ?
            `<!--@any[${token}]--> <!--@notany--> <!--@endany-->` :
            `@any[${token}] @notany @endany`)}
        </div>
      </div>
    );
  }

  renderButton(text: string, canClick: string | undefined, buildPattern: (key: string) => string) {
    return <input type="button" disabled={!!canClick} className="btn btn-light btn-sm sf-button"
      title={canClick} value={text}
      onClick={() => this.props.onInsert(buildPattern(this.state.currentToken ? this.state.currentToken.fullKey : ""))} />;
  }


  canElement(): string | undefined {
    let token = this.state.currentToken;

    if (token == undefined)
      return TemplateTokenMessage.NoColumnSelected.niceToString();

    if (token.type.isCollection)
      return TemplateTokenMessage.YouCannotAddIfBlocksOnCollectionFields.niceToString();

    if (hasAnyOrAll(token))
      return TemplateTokenMessage.YouCannotAddBlocksWithAllOrAny.niceToString();

    return undefined;
  }


  canIf(): string | undefined {
    let token = this.state.currentToken;

    if (token == undefined)
      return TemplateTokenMessage.NoColumnSelected.niceToString();

    if (token.type.isCollection)
      return TemplateTokenMessage.YouCannotAddIfBlocksOnCollectionFields.niceToString();

    if (hasAnyOrAll(token))
      return TemplateTokenMessage.YouCannotAddBlocksWithAllOrAny.niceToString();

    return undefined;
  }

  canForeach(): string | undefined {

    let token = this.state.currentToken;

    if (token == undefined)
      return TemplateTokenMessage.NoColumnSelected.niceToString();

    if (token.type.isCollection)
      return TemplateTokenMessage.YouHaveToAddTheElementTokenToUseForeachOnCollectionFields.niceToString();

    if (token.key != "Element" || token.parent == undefined || !token.parent.type.isCollection)
      return TemplateTokenMessage.YouCanOnlyAddForeachBlocksWithCollectionFields.niceToString();

    if (hasAnyOrAll(token))
      return TemplateTokenMessage.YouCannotAddBlocksWithAllOrAny.niceToString();

    return undefined;
  }

  canAny() {

    let token = this.state.currentToken;

    if (token == undefined)
      return TemplateTokenMessage.NoColumnSelected.niceToString();

    if (hasAnyOrAll(token))
      return TemplateTokenMessage.YouCannotAddBlocksWithAllOrAny.niceToString();

    return undefined;
  }
}





