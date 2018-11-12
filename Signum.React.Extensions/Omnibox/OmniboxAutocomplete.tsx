﻿import * as React from 'react'
import { AbortableRequest } from '@framework/Services';
import * as Navigator from '@framework/Navigator'
import { Typeahead, ErrorBoundary } from '@framework/Components'
import * as OmniboxClient from './OmniboxClient'
import { OmniboxMessage } from './Signum.Entities.Omnibox'
import '@framework/Frames/MenuIcons.css'

export interface OmniboxAutocompleteProps {
  inputAttrs?: React.HTMLAttributes<HTMLInputElement>;
}

export default class OmniboxAutocomplete extends React.Component<OmniboxAutocompleteProps>
{
  handleOnSelect = (result: OmniboxClient.OmniboxResult, e: React.KeyboardEvent<any> | React.MouseEvent<any>) => {

    this.abortRequest.abort();

    const ke = e as React.KeyboardEvent<any>;
    if (ke.keyCode && ke.keyCode == 9) {
      return OmniboxClient.toString(result);
    }
    e.persist();

    const promise = OmniboxClient.navigateTo(result);
    if (promise) {
      promise
        .then(url => {
          if (url)
            Navigator.pushOrOpenInTab(url, e);
        }).done();
    }
    this.typeahead.blur();

    return null;
  }

  abortRequest = new AbortableRequest((ac, query: string) => OmniboxClient.API.getResults(query, ac));

  typeahead!: Typeahead;

  render() {

    let inputAttr = { tabIndex: -1, placeholder: OmniboxMessage.Search.niceToString(), ...this.props.inputAttrs };

    return (
      <ErrorBoundary>
        <Typeahead ref={ta => this.typeahead = ta!} getItems={str => this.abortRequest.getData(str)}
          renderItem={item => OmniboxClient.renderItem(item as OmniboxClient.OmniboxResult)}
          onSelect={(item, e) => this.handleOnSelect(item as OmniboxClient.OmniboxResult, e)}
          inputAttrs={inputAttr}
          minLength={0} />
      </ErrorBoundary>
    );
  }
}






