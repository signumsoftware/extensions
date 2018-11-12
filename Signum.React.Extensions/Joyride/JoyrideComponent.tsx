﻿/// <reference path="joyride.d.ts"/>
import * as React from 'react'
import Joyride, { Step, StepStyles } from 'react-joyride'
import { JoyrideEntity, JoyrideStepPosition, JoyrideType, JoyrideStepType, JoyrideStepStyleEntity, JoyrideMessage } from './Signum.Entities.Joyride'

import 'react-joyride/lib/react-joyride-compiled.css'

export interface JoyrideComponentProps {
  joyride: JoyrideEntity;
}

export interface JoyrideComponentState {

}

export default class JoyrideComponent extends React.Component<JoyrideComponentProps, JoyrideComponentState> {

  joyride: Joyride | null | undefined;

  constructor(props: JoyrideComponentProps) {
    super(props);
    this.state = {};
  }

  render() {
    const j = this.props.joyride;

    return (
      <Joyride
        ref={c => this.joyride = c}
        stepIndex={0}
        type={this.toJoyrideType(j.type)}
        showSkipButton={j.showSkipButton}
        showStepsProgress={j.showStepsProgress}
        keyboardNavigation={j.keyboardNavigation}
        autoStart={true}
        steps={this.toJoyrideSteps(j)}
        run={false}
        locale={{
          back: JoyrideMessage.Back.niceToString(),
          close: JoyrideMessage.Close.niceToString(),
          last: JoyrideMessage.Last.niceToString(),
          next: JoyrideMessage.Next.niceToString(),
          skip: JoyrideMessage.Skip.niceToString()
        }}
        debug={j.debug}
      />
    );
  }

  toJoyrideSteps(joyrideEntity: JoyrideEntity): Step[] {
    return joyrideEntity.steps.map(s => ({
      title: s.element.title,
      text: s.element.text,
      selector: s.element.selector,
      style: this.toJoyrideStepStyle(s.element.style),
      position: this.toJoyridePosition(s.element.position),
      type: this.toJoyrideStepType(s.element.type),
      allowClicksThruHole: s.element.allowClicksThruHole,
      isFixed: s.element.isFixed
    } as Step));
  }

  toJoyrideStepStyle(joyrideStepStyle: JoyrideStepStyleEntity | null | undefined): StepStyles {
    if (joyrideStepStyle)
      return {
        backgroundColor: joyrideStepStyle.backgroundColor,
        color: joyrideStepStyle.color,
        mainColor: joyrideStepStyle.mainColor,
        borderRadius: joyrideStepStyle.borderRadius,
        textAlign: joyrideStepStyle.textAlign,
        width: joyrideStepStyle.width
      } as StepStyles;

    return {};
  }

  toJoyridePosition(position: JoyrideStepPosition | undefined): "top" | "top-left" | "top-right" | "bottom" | "bottom-left" | "bottom-right" {
    switch (position) {
      case ("Top"):
        return "top";
      case ("TopLeft"):
        return "top-left";
      case ("TopRight"):
        return "top-right";
      case ("Bottom"):
        return "bottom";
      case ("BottomLeft"):
        return "bottom-left";
      case ("BottomRight"):
        return "bottom-right";
      default:
        throw new Error(`The joyride position ${position} is not yet supported.`);
    }
  }

  toJoyrideType(type: JoyrideType | undefined): "continuous" | "single" {
    switch (type) {
      case ("Continuous"):
        return "continuous";
      case ("Single"):
        return "single";
      default:
        throw new Error(`The joyride type ${type} is not yet supported.`);
    }
  }

  toJoyrideStepType(type: JoyrideStepType | undefined): string {
    switch (type) {
      case ("Click"):
        return "click";
      case ("Hover"):
        return "hover";
      default:
        throw new Error(`The joyride step type ${type} is not yet supported.`);
    }
  }
}
