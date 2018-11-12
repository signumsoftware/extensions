﻿import * as React from 'react'
import { Dic } from '@framework/Globals';
import { Lite, is } from '@framework/Signum.Entities'
import { CultureInfoEntity } from '../Basics/Signum.Entities.Basics'
import * as CultureClient from './CultureClient'
import { UncontrolledDropdown, DropdownToggle, DropdownMenu, DropdownItem } from '@framework/Components';

export interface CultureDropdownProps {
}

export interface CultureDropdownState {
  cultures?: { [name: string]: Lite<CultureInfoEntity> };
}

export default class CultureDropdown extends React.Component<CultureDropdownProps, CultureDropdownState> {
  constructor(props: CultureDropdownProps) {
    super(props);
    this.state = {};
  }

  componentWillMount() {
    CultureClient.getCultures(false)
      .then(cultures => this.setState({ cultures: cultures }))
      .done();
  }

  handleSelect = (c: Lite<CultureInfoEntity>) => {
    CultureClient.changeCurrentCulture(c);
  }

  render() {
    const cultures = this.state.cultures;

    if (!cultures)
      return null;

    const current = CultureClient.currentCulture;

    const pair = Dic.map(cultures, (name, c) => ({ name, c })).filter(p => is(p.c, current)).singleOrNull();

    return (
      <UncontrolledDropdown id="cultureDropdown" data-culture={pair && pair.name} nav inNavbar>
        <DropdownToggle nav caret>
          {current.nativeName}
        </DropdownToggle>
        <DropdownMenu right>
          {Dic.map(cultures, (name, c, i) =>
            <DropdownItem key={i} data-culture={name} disabled={is(c, current)} onClick={() => this.handleSelect(c)}>
              {c.toStr}
            </DropdownItem>
          )}
        </DropdownMenu>
      </UncontrolledDropdown>
    );
  }
}
