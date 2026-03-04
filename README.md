# Core Utilities

## Install

VCC: https://nikkyai.github.io/vpm  
using https://vrc-get.anatawa12.com/en/alcom/ is highly recommended


## About

Provides core utilities for Kintec Controls

Toggles, Triggers and Radio Buttons

Provides a easy to customize base classes for anything activated by a control surface
inherit from `FloatDriver` for faders/sliders, `IntDriver` with radia buttons, `BoolDriver` for toggles and `TriggerDriver` for buttons

the control surfaces load the Drivers dynamically at startup, so you jsut need to add a new component

integrated with TXL for logging and authorization checks



## builtin Drivers

this list is incomplete, you may look at the sourcecode for a complete list
names may change as refactors unify the naming scheme

- animators
    - AnimatorBoolDriver
    - AnimatorIntDriver
    - AnimatorFloatDriver
- audiolink
    - AudiolinkDriver
- blendshape
    - FloatBlendShapeDriver
- material
    - FloatMaterialPropertyDriver
    - IntMaterialPropertyDriver
- text
    - FloatTextDriver
    - IntTextDriver
- udon
    - FloatUdonBehaviourDriver
- BoolSyncedDriver
- FloatToBOolDriver
- ObjectToggleDriver
- TriggerRandomDriver

