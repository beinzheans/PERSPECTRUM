# Editor Documentation

The Editor is a built-in feature in the game to make the creation of levels (charts) easier. With the Editor having no built-in tutorial in the game due to it's complexity and future features to be added, this documentation serves to:

- define the [key terms and layouts](#key-terms--layout) used in the Editor that will be used in throughout this documentation file
- list the [controls](#control-scheme) used in the Editor and describe their actions
- list, describe and demonstrate [all the tools](#all-editor-tools) currently in the Editor

The documentation is written under the assumption that you are familiar with the gameplay.

## Key Terms & Layout

### Editor Object

An Editor Object is defined as any object that can be placed by the Editor.

### Editor Chart

An Editor Chart contains a collection of [Editor Objects](#editor-object), audio data (audio file, only .mp3 supported) and chart metadata. The Editor Chart is saved as a *.psr* file.

### Editor Preview Time

The Editor Preview Time is defined as the current time of the preview of the current [Editor Chart](#editor-chart). Changing the preview time will change the preview of the [Editor Chart](#editor-chart).

#### Schematic

Below is a schematic of the Editor. This will be an useful reference throughout the documentation.

<img width="2560" height="1440" alt="image" src="https://github.com/user-attachments/assets/a04cb84d-d59d-4332-a8d1-819e4fdbe269" />

### Object Toolkit Panel

The Object Toolkit panel (left panel) provides all tools necessary to create new or manipulate existing [Editor Objects](#editor-object). There are three sections under the Object Toolkit panel:

#### 1. Editor Objects Section

   This section defines what type of [Editor Object](#editor-object) is currently active. Only one type can be selected at any time. This also changes the currently avaliable tools provided by the [Object Tools](#3-object-tools-section).
   - **A**: Red Note. The player must become Red to match this note.
   - **B**: Blue Note. The player must become Blue to match this note.
   - **Bomb**: Bomb Note. The player must dodge this note.
   - **Point**: Editor Point.
   - **Line**: Editor Line.
   - **Marker**: Editor Marker. Editor Markers are a key part in splitting your chart into different sections with a designated BPM. This influences the behavior of the [Timeline Panel](#timeline-panel). 

#### 2. Selection Tool Section

   This section allows for the simple manipulation of selected [Editor Objects](#editor-object).
   - **Mirror Vertical**: Mirrors all selected [Editor Objects](#editor-object) vertically (ie. reflect about the horizontal axis).
   - **Mirror Horizontal**: Mirrors all selected [Editor Objects](#editor-object) horizontally (ie. reflect about the vertical axis).
   - **Rotate 90 CW**: Rotates all selected [Editor Objects](#editor-object) 90 degrees in the clockwise direction about the center of the [Editor Preview Panel](#editor-preview-panel)
   - **Rotate 90 Anti-CW**: Rotates all selected [Editor Objects](#editor-object) 90 degrees in the anti-clockwise direction about the center of the [Editor Preview Panel](#editor-preview-panel).

   > [!IMPORTANT]
   > Rotation of [Editor Objects](#editor-object) may not give expected results. Since the game enforces a 16:9 play area, when rotating an [Editor Object](#editor-object), it is entirely possible for the [Editor Object](#editor-object) to be placed outside of the [Editor Preview Panel](#editor-preview-panel) in order to preserve the distance from the [Editor Object](#editor-object) to the center of the [Editor Preview Panel](#editor-preview-panel) after rotation.

#### 3. Object Tools Section

This section provides you with tools for the creation or manipulation of [Editor Objects](#editor-object). The avalaible tools provided by this section is defined by the [Editor Objects Section](#1-editor-objects-section). All of the tools under this section is placed under the [All Editor Tools](#all-editor-tools) section.

### Timeline Panel

The Timeline panel (bottom panel) serves to provide charters an easier way to synchronize the [Editor Objects](#editor-object) to the loaded audio. The Timeline:
   - shows the current [Editor Preview Time](#editor-preview-time) as well as the beats near this time.
   - shows the visualized beats as yellow lines, where a large yellow line is one beat, while the small yellow lines are the sub-beats.
   - shows the sections of the charts divided by [Editor Markers](#1-editor-objects-section), displaying the current section's name, BPM and message to display (when in [playback mode](#1-playback-options-section))

### Timing Toolkit Panel

The Timing Toolkit Panel (right panel) aims to help charters to control the timing of the Editor

#### 1. Playback Options Section

   Controls the behavior for the Editor playback. The playback automatically moves the [Editor Preview Time](#editor-preview-time) as if it is in gameplay.
   - **At Start**: Forces the [Editor Preview Time](#editor-preview-time) to be at the beginning of the [Editor Chart](#editor-chart) (ie, the 0 second mark) and starts the playback at the designated playback speed.
   - **At Current Time**: Starts the playback at the designated playback speed at the [Editor Preview Time](#editor-preview-time).
   - **At Current Section**: Forces the [Editor Preview Time](#editor-preview-time) to be at the beginning of the [current section](#timeline-panel) and starts the playback at the designated playback speed.
   - **Playback Speed**: Controls the playback speed. Must be a positive number, otherwise the playback speed will default to 1.
  
#### 2. Cursor Options Section

   Controls the behavior of the cursor inside the [Editor Preview Panel](#editor-preview-panel).
   - **No Snap**: Does not snap the cursor. This allows for [Editor Objects](#editor-object) to be placed exactly where your cursor is.
   - **Snap To Grid**: Snaps the cursor to a pre-defined 20x20 grid inside the [Editor Preview Panel](#editor-preview-panel). A slightly transparent cursor will visualize the position where [Editor Objects](#editor-object) will be placed.

   > [!IMPORTANT]
   > The grid intervals are NOT square. Since the game enforces a 16:9 play area, the distance travelled in one step on the horizontal axis is larger than that on the vertical axis.

#### 3. Chart Options Section

   Provides control for loading and saving the current [Editor Chart](#editor-chart).
   - **Load Audio**: Opens the file browser and loads the selected *.mp3* file into the Editor.
   - **Save Editor Chart**: Opens the file browser and saves the current [Editor Chart](#editor-chart) as a *.psr* file at the selected path and name.
   - **Load Editor Chart**: Opens the file browser and loads the selected *.psr* file into the Editor.
   - **Exit Editor**: Returns to the title screen of the game. You can also exit by opening the settings menu with the Escape key.

### Chart Metadata Panel

   The Chart Metadata Panel

### Editor Preview Panel

## Control Scheme

## All Editor Tools
