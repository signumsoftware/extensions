﻿import * as React from 'react'
import CodeMirrorComponent from './CodeMirrorComponent'
import * as CodeMirror from 'codemirror'

import "codemirror/lib/codemirror.css"
import "codemirror/addon/dialog/dialog.css"
import "codemirror/addon/display/fullscreen.css"
import "codemirror/addon/hint/show-hint.css"
import "codemirror/lib/codemirror"
import "codemirror/mode/javascript/javascript"
import "codemirror/addon/comment/comment"
import "codemirror/addon/comment/continuecomment"
import "codemirror/addon/dialog/dialog"
import "codemirror/addon/display/fullscreen"
import "codemirror/addon/edit/closebrackets"
import "codemirror/addon/edit/matchbrackets"
import "codemirror/addon/hint/show-hint"
import "codemirror/addon/hint/javascript-hint"
import "codemirror/addon/search/match-highlighter"
import "codemirror/addon/search/search"
import "codemirror/addon/search/searchcursor"

interface JavascriptCodeMirrorProps {
  code: string;
  onChange?: (code: string) => void;
}


export default class JavascriptCodeMirror extends React.Component<JavascriptCodeMirrorProps> {

  codeMirrorComponent!: CodeMirrorComponent;

  render() {
    const options = {
      lineNumbers: true,
      viewportMargin: Infinity,
      mode: "javascript",
      extraKeys: {
        "Ctrl-Space": "autocomplete",
        "Ctrl-K": (cm: any) => cm.lineComment(cm.getCursor(true), cm.getCursor(false)),
        "Ctrl-U": (cm: any) => cm.uncomment(cm.getCursor(true), cm.getCursor(false)),
        "Ctrl-I": (cm: any) => cm.autoFormatRange(cm.getCursor(true), cm.getCursor(false)),
        "F11": (cm: any) => cm.setOption("fullScreen", !cm.getOption("fullScreen")),
        "Esc": (cm: any) => {
          if (cm.getOption("fullScreen"))
            cm.setOption("fullScreen", false);
        }
      }
    } as CodeMirror.EditorConfiguration;

    (options as any).highlightSelectionMatches = true;
    (options as any).matchBrackets = true;

    return (
      <div className="small-codemirror">
        <CodeMirrorComponent value={this.props.code} ref={cm => this.codeMirrorComponent = cm!}
          options={options}
          onChange={this.props.onChange} />
      </div>
    );
  }
}
