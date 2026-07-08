# Editor Documentation

The Editor is a built-in feature in the game to make the creation of levels (charts) easier. With the Editor having no built-in tutorial in the game due to it's complexity and future features to be added, this documentation serves to:

- define the [key terms and layouts](#key-terms--layout) used in the Editor that will be used in throughout this documentation file
- list the [controls](#control-scheme) used in the Editor and describe their actions
- list, describe and demonstrate [all the tools](#all-editor-tools) currently in the Editor

The documentation is written under the assumption that you are familiar with the gameplay.

## Key Terms & Layout

### Editor Object

An Editor Object is defined as any object that can be placed by the Editor. Each Editor Object will have their own respective Render Time.

### Editor Chart

An Editor Chart contains a collection of [Editor Objects](#editor-object), audio data (audio file, only .mp3 supported) and chart metadata. The Editor Chart is saved as a `.psr` file.

### Editor Preview Time

The Editor Preview Time is defined as the current time of the preview of the current [Editor Chart](#editor-chart). Changing the preview time will change the preview of the [Editor Chart](#editor-chart).

### Editor Lookahead Time

The Editor Lookahead Time is defined as the time interval that we add on top of the [Editor Preview Time](#editor-preview-time) that the [Editor Preview Panel](#editor-preview-panel) will visualize.

#### Schematic

Below is a screenshot of the Editor. This will be an useful reference throughout the documentation.

<img width="2560" height="1440" alt="image" src="https://github.com/user-attachments/assets/db688301-1a7b-4582-bc33-fd6367a53ff2" />


### Object Toolkit Panel

The Object Toolkit panel (left panel) provides all tools necessary to create new or manipulate existing [Editor Objects](#editor-object).

#### 1. Editor Objects Section

   This section defines what type of [Editor Object](#editor-object) is currently active. Only one type can be selected at any time. This also changes the currently avaliable tools provided by the [Object Tools](#3-object-tools-section).
   - **A**: Red Note. The player must become Red to match this note.
   - **B**: Blue Note. The player must become Blue to match this note.
   - **Bomb**: Bomb Note. The player must dodge this note.
   - **Point**: Editor Point.
   - **Line**: Editor Line. A line is defined between two [Editor Objects (except for the line type)](#editor-object)
   - **Marker**: Editor Marker. Editor Markers are a key part in splitting your chart into different sections with a designated BPM. This influences the behavior of the [Timeline Panel](#timeline-panel).

   Refer to the [Control Scheme](#control-scheme) for help on creating [Editor Objects](#editor-object).

   Refer to the [All Editor Tools](#all-editor-tools) for help on deleting Editor Markers.

#### 2. Selection Tool Section

   This section allows for the simple manipulation of selected [Editor Objects](#editor-object). You can choose any combination of the below:
   
   - **Mirror Vertical**: Mirrors all selected [Editor Objects](#editor-object) vertically (ie. reflect about the horizontal axis).
   - **Mirror Horizontal**: Mirrors all selected [Editor Objects](#editor-object) horizontally (ie. reflect about the vertical axis).
   - **Rotate 90 CW**: Rotates all selected [Editor Objects](#editor-object) 90 degrees in the clockwise direction about the center of the [Editor Preview Panel](#editor-preview-panel)
   - **Rotate 90 Anti-CW**: Rotates all selected [Editor Objects](#editor-object) 90 degrees in the anti-clockwise direction about the center of the [Editor Preview Panel](#editor-preview-panel).

   Refer to the [Control Scheme](#control-scheme) for help on performing the above actions after selecting the combination.
   
   > [!IMPORTANT]
   > **Rotation of [Editor Objects](#editor-object) may not give expected results**. Since the game enforces a 16:9 play area, when rotating an [Editor Object](#editor-object), it is entirely possible for the [Editor Object](#editor-object) to be placed outside of the [Editor Preview Panel](#editor-preview-panel) in order to preserve the distance from the [Editor Object](#editor-object) to the center of the [Editor Preview Panel](#editor-preview-panel) after rotation.

#### 3. Object Tools Section

This section provides you with tools for the creation or manipulation of [Editor Objects](#editor-object). The avalaible tools provided by this section is defined by the [Editor Objects Section](#1-editor-objects-section). All of the tools under this section is placed under the [All Editor Tools](#all-editor-tools) section.

### Timeline Panel

The Timeline panel (bottom panel) serves to provide charters an easier way to synchronize the [Editor Objects](#editor-object) to the loaded audio. The Timeline:
   - shows the current [Editor Preview Time](#editor-preview-time) as well as the beats near this time.
   - shows the visualized beats as yellow lines, where a large yellow line is one beat, while the small yellow lines are the sub-beats.
   - shows the sections of the charts divided by [Editor Markers](#1-editor-objects-section), displaying the current section's name, BPM and message to display (when in [playback mode](#1-playback-options-section))

If the current section has an unknown BPM, there will be no visualized beats.

To control the number of sub-beats, refer to [Control Scheme](#control-scheme).

### Control Toolkit Panel

The Control Toolkit Panel (right panel) aims to help charters to control the behavior of the Editor.

#### 1. Playback Options Section

   Controls the behavior for the Editor playback. The playback automatically moves the [Editor Preview Time](#editor-preview-time) as if it is in gameplay.
   - **At Start**: Forces the [Editor Preview Time](#editor-preview-time) to be at the beginning of the [Editor Chart](#editor-chart) (ie, the 0 second mark) and starts the playback at the designated playback speed.
   - **At Current Time**: Starts the playback at the designated playback speed at the [Editor Preview Time](#editor-preview-time).
   - **At Current Section**: Forces the [Editor Preview Time](#editor-preview-time) to be at the beginning of the [current section](#timeline-panel) and starts the playback at the designated playback speed.
   - **Playback Speed**: Controls the playback speed. Must be a positive number, otherwise the playback speed will default to 1.

   To start playback, refer to the [Control Scheme](#control-scheme).
   
#### 2. Cursor Options Section

   Controls the behavior of the cursor inside the [Editor Preview Panel](#editor-preview-panel).
   - **No Snap**: Does not snap the cursor. This allows for [Editor Objects](#editor-object) to be placed exactly where your cursor is.
   - **Snap To Grid**: Snaps the cursor to a pre-defined 20x20 grid inside the [Editor Preview Panel](#editor-preview-panel).

   > [!IMPORTANT]
   > **The grid size are NOT square**. Since the game enforces a 16:9 play area, the distance travelled in one step on the horizontal axis is larger than that on the vertical axis.

#### 3. Chart Options Section

   Provides control for the loading and saving the [Editor Chart](#editor-chart).
   - **Load Audio**: Opens the file browser and loads the selected `.mp3` file into the Editor.
   - **Save Editor Chart**: Opens the file browser and saves the current [Editor Chart](#editor-chart) as a `.psr` file at the selected path and name.
   - **Load Editor Chart**: Opens the file browser and loads the selected `.psr` file into the Editor.
   - **Exit Editor**: Returns to the Title Screen of the game. You can also exit by opening the settings menu with the Escape key.

### Chart Metadata Panel

   The Chart Metadata Panel (top panel) provides charters a simple series of input fields to fill in the metadata of the [Editor Chart](#editor-chart).
   - **Chart Title**: The title used in the Chart Select screen.
   - **Chart Mapper(s)**: The name(s) of people that worked on this chart.
   - **Chart Difficulty**: The difficulty number of the chart.
   - **Song Name**: The name of the song used in the Chart.
   - **Song Artist(s)**: The name(s) of artist(s) that created the song used in the Chart.

   > [!WARNING]
   > **Respect copyright**. If you are distributing your chart to other people online, please get permission from the song artist(s) to do so. I have received permission to use the songs from the artists, so please do the same.
   > At the very least, correctly credit the song and song artist(s).
   > You can consider using DMCA-free or royalty-free songs.

### Editor Preview Panel

   The Editor Preview Panel (center panel) provide charters a preview of the current [Editor Chart](#editor-chart) at the [Editor Preview Time](#editor-preview-time). The Preview:
   - shows a visualization of every [Editor Object](#editor-object) inside the [Editor Chart](#editor-chart) from [`Editor Preview Time`](#editor-preview-time) to [`Editor Preview Time + Editor Lookahead Time`](#editor-lookahead-time).
   - allows for the selection of [Editor Objects](#editor-object).
   - shows a visualization of the position that [Editor Objects](#editor-object) will be placed at using a transparent cursor.

   > [!WARNING]
   > The Editor Preview Panel uses object-pooling approach to display the visualization of [Editor Objects](#editor-object). Each [Editor Object](#editor-object) will have their respective pools. If you are placing an extreme number of notes, or using a restrictively long [Editor Lookahead Time](#editor-lookahead-time), it is entirely possible that the pool is completely used up. This will not break the Editor but will lag the Editor due to the number of [Editor Objects](#editor-object) and the warning messages being recorded in the player log.
 
## Control Scheme

Below are the details the control scheme of the Editor. **These keys are NOT rebindable in this version.**

### Editor Object Controls

#### Universal Controls


| Control | Name | Description | Demonstration |
| --- | --- | --- | --- |
| `Right Click` | Place Object | Places an [Editor Object](#editor-object) at the [Editor Preview Time](#editor-preview-time) according to the [current selected type](#1-editor-objects-section).<br><br>**Lines**: Select any two [Editor Objects](#editor-object) to generate a line. The order of selection matters.<br><br>**Markers**: You must define a BPM in order to place a marker. The marker placed can be seen in the [Timeline panel](#timeline-panel). | <img width="400" height="225" alt="2026-07-08 09-17-59" src="https://github.com/user-attachments/assets/a19fe392-82aa-40a5-9cc9-1df16f4ee479" /> |
| `Left Click` | Select Objects | Selects a visualized [Editor Object](#editor-object) on the [Editor Preview Panel](#editor-preview-panel). The selected [Editor Objects](#editor-object) are highlighted green. | <img width="400" height="225" alt="2026-07-08 09-32-53" src="https://github.com/user-attachments/assets/34006efd-3893-4534-baf0-35d146b70c9b" /> |
| `X` | Delete Selected | Deletes all currently selected [Editor Objects](#editor-object).<br><br>**Markers** can not be deleted since they can not be selected. For help on deleting markers, refer to [All Editor Tools](#all-editor-tools) | <img width="400" height="225" alt="2026-07-08 09-43-05" src="https://github.com/user-attachments/assets/03d0e1d8-b5a3-4fbf-bdeb-c9bdf3e7c0e7" /> |
| `A` | Select All | Selects all currently visualized [Editor Objects](#editor-object) on the [Editor Preview Panel](#editor-preview-panel). | <img width="400" height="225" alt="2026-07-08 09-58-58" src="https://github.com/user-attachments/assets/3b5116be-d40b-498f-8fa1-6b2f2e7bfb71" /> |
| `Q` | Tool Decrement Input | Invokes a decrement event to the currently active tool. See [All Editor Tools](#all-editor-tools) to see specific implementations. | N. A. |
| `E` | Tool Increment Input | Invokes an increment event to the current active tool. See [All Editor Tools](#all-editor-tools) to see specific implementations. | N. A. |
| `Ctrl`-`A` | Deselect All | Deselects all currently selected [Editor Objects](#editor-object), even if it is not visualized on the [Editor Preview Panel](#editor-preview-panel). | <img width="400" height="225" alt="2026-07-08 10-03-19" src="https://github.com/user-attachments/assets/86456593-088d-44fc-ac00-e30ea0df875c" /> |
| `Ctrl`-`M` | Move | Moves all currently selected [Editor Objects](#editor-object) according to the [Selection Tool](#2-selection-tool-section) options. | <img width="400" height="225" alt="2026-07-0810-30-53-ezgif com-video-to-gif-converter" src="https://github.com/user-attachments/assets/041384cd-2355-4811-851e-5c705fbbcf57" /> |
| `Alt`-`X` | Fix Horizontal Axis | Forces the [Editor Preview Cursor](#editor-preview-panel) to move only along the horizontal axis |<img width="400" height="225" alt="2026-07-0810-48-54-ezgif com-video-to-gif-converter" src="https://github.com/user-attachments/assets/500efee8-c504-4d6a-848d-add5e095b421" /> |
| `Alt`-`Y` | Fix Vertical Axis | Forces the [Editor Preview Cursor](#editor-preview-panel) to move only along the vertical axis | <img width="400" height="225" alt="2026-07-0810-49-06-ezgif com-video-to-gif-converter" src="https://github.com/user-attachments/assets/947c11b9-9f53-4587-83a5-e13211e71b0a" /> |
| `Ctrl`-`C` | Copy | Copies all currently selected [Editor Objects](#editor-object). This deselects all currently selected [Editor Objects](#editor-object). | <img width="400" height="225" alt="2026-07-08 10-08-22" src="https://github.com/user-attachments/assets/d912af8e-84c4-4e29-9751-5af6768fee82" /> |
| `Ctrl`-`X` | Cut | Cuts all currently selected [Editor Objects](#editor-object). This deletes all currently selected [Editor Objects](#editor-object). | <img width="400" height="225" alt="2026-07-08 10-10-57" src="https://github.com/user-attachments/assets/8b743c87-c0d7-4ba1-bd6e-204f423ce7de" /> |
| `Ctrl`-`V` | Paste | Pastes the selected [Editor Objects](#editor-object) performed in the Copy or Cut at the [Editor Preview Time](#editor-preview-time) | <img width="400" height="225" alt="2026-07-08 10-14-08" src="https://github.com/user-attachments/assets/bdc5c831-4535-4e8b-8274-08948c447471" /> |
| `Ctrl`-`Z` | Undo | Undoes the previous control performed. | <img width="400" height="225" alt="2026-07-0810-17-28-ezgif com-video-to-gif-converter" src="https://github.com/user-attachments/assets/e9948ab1-65b1-4d67-9cf0-7e1c6de52be2" /> |
| `Ctrl`-`Shift`-`Z` | Redo | Redoes the previous undo operation performed. | <img width="400" height="225" alt="2026-07-0810-24-35-ezgif com-video-to-gif-converter" src="https://github.com/user-attachments/assets/5670e94a-8d85-4e38-b984-df174c651247" /> |

#### Note Controls


| Control | Name | Description | Demonstration |
| --- | --- | --- | --- |
| `Alt`-`Scroll` | Change Note Size | Changes the note size of the notes to be placed.<br><br>**Does not change the size of currently selected notes.** To change the size of selected notes, refer to [All Editor Tools](#all-editor-tools). | <img width="400" height="225" alt="2026-07-0810-44-56-ezgif com-video-to-gif-converter" src="https://github.com/user-attachments/assets/58f46bdb-61eb-4137-949e-d42fd8e286ba" /> |


### Timing Controls


| Control | Name | Action Done | Demonstration |
| --- | --- | --- | --- |
| `Scroll` | Move Time to Beat | Moves the [Editor Preview Time](#editor-preview-time) to the next beat visualized in the [Timeline Panel](#timeline-panel). If no beats are visualized, nothing is performed.<br><br>[Editor Objects](#editor-object) becomes fully opaque when [Editor Preview Time](#editor-preview-time) is the same as the [Editor Objects'](#editor-object) render time. | <img width="400" height="225" alt="2026-07-08 09-47-50" src="https://github.com/user-attachments/assets/e489a45a-4fd6-439b-bae7-b7fa76a57a7c" /> |
| `Shift`-`Scroll` | Move Time by Delta | Moves the [Editor Preview Time](#editor-preview-time) by a delta without snapping to beats. This delta can be defined in the Settings menu. | <img width="400" height="225" alt="2026-07-0810-55-56-ezgif com-video-to-gif-converter" src="https://github.com/user-attachments/assets/10d0fb15-7e23-479c-b6b7-1e86c0e88735" /> |
| `Ctrl`-`Scroll` | Beat Subdivision | Adjusts the current number of sub-beats that make up one beat. This is represented by the [Timeline Panel](#timeline-panel) as a ratio of `1 : n`, where `n` is the number of sub-beats. | <img width="400" height="225" alt="2026-07-0810-59-36-ezgif com-video-to-gif-converter" src="https://github.com/user-attachments/assets/fbd37480-fb9b-4978-9b4e-41132e321f85" /> |
| `Space` | Start Playback | Starts the playback according to the [Playback Options](#1-playback-options-section). | <img width="400" height="225" alt="2026-07-0811-05-47-ezgif com-video-to-gif-converter" src="https://github.com/user-attachments/assets/0fb891f4-c7c6-4a4f-ac2a-e36210502b73" /> |



## All Editor Tools

Below are the details and usages of the Editor tools.

### Note Tools (Red, Blue, Bomb)

#### 1. Change to Type A

Changes all currently selected notes to [A (Red) notes](#1-editor-objects-section).

#### 2. Change to Type B

Changes all currently selected notes to [B (Blue) notes](#1-editor-objects-section).

#### 3. Change to Bomb

Changes all currently selected notes to [Bomb notes](#1-editor-objects-section).

#### 4. Add Size Delta

Adds a size delta according to the input field value. Keep in mind that sizes range from 0 to 1.

Press on the button to enable this tool. You can decrease or increase the size by a delta defined by the input field using the [Tool Increment/Decrement Input](#control-scheme).

<img width="853" height="480" alt="2026-07-0811-32-02-ezgif com-video-to-gif-converter" src="https://github.com/user-attachments/assets/ef60acd5-136b-4e00-8923-0ec5c7dec395" />


### Point Tools

#### 1. Set Radius

Sets the radius to be used when [Create Circle](#6-create-circle) is clicked.

#### 2. Set Initial Angle

Sets the initial angle to be used when [Create Circle](#6-create-circle) is clicked.

#### 3. Set Final Angle

Sets the final angle to be used when [Create Circle](#6-create-circle) is clicked.

#### 4. Set Number Of Points

Sets the number of points to sample (generate) when [Create Circle](#6-create-circle) or [Create Arc](#7-create-arc) is clicked.

#### 5. Set Time Offset

Sets the time offset between points when [Create Circle](#6-create-circle) is clicked.

> [!CAUTION]
> You are not advised to use this. This feature will be improved in later releases to use beat offset instead of time offset.
> 
> You can instead make use of the Copy, Cut and Paste if you want to offset the time.

#### 6. Create Circle

Creates a circle with a defined center, radius, initial angle, final angle, number of points and time offset.
A center is defined by the currently selected [Editor Point(s)](#1-editor-objects-section).

<img width="853" height="480" alt="2026-07-0811-43-44-ezgif com-video-to-gif-converter (1)" src="https://github.com/user-attachments/assets/1a05c9e5-d374-4d2e-9255-446fd3852630" />


> [!IMPORTANT]
> The angles are using standard math convention (polar coordinates). "0" is the positive horizontal axis (to the right, +x-axis). Positive values indicate an anti-clockwise direction. Angles are measured from the positive horizontal axis (to the right, +x-axis).


#### 7. Create Arc

Creates an arc with a defined line, focus point and number of points.

<img width="853" height="480" alt="2026-07-0811-48-28-ezgif com-video-to-gif-converter" src="https://github.com/user-attachments/assets/2495febc-a5ea-4a88-9488-c1873e2966a7" />


#### 8. Convert To Hitbox

Converts all selected [Editor Points](#1-editor-objects-section) to be type A Hitbox (Red Note) using the current [Note Size](#control-scheme). You can then use [Note Tools](#note-tools-red-blue-bomb) for further changes.

### Line Tools

#### 1. Generate Exterior

Fills the exterior of a shape bounded by the currently selected lines with [Bombs](#1-editor-objects-section).

<img width="853" height="480" alt="2026-07-0821-03-20-ezgif com-video-to-gif-converter" src="https://github.com/user-attachments/assets/0a812873-f6d0-4406-9c5a-2a1ffaed1998" />


#### 2. Generate Interior

Fills  the interior of a shape bounded by the currently selected lines with [Bombs](#1-editor-objects-section).

<img width="853" height="480" alt="2026-07-0821-03-31-ezgif com-video-to-gif-converter" src="https://github.com/user-attachments/assets/e9aaff53-43b0-4acb-b2c7-d2466d7abf3e" />


#### 3. Subdivide Line

Subdivide the line into `n` points, including the end points of the line.

<img width="853" height="480" alt="2026-07-0821-03-48-ezgif com-video-to-gif-converter" src="https://github.com/user-attachments/assets/a4682e95-9db8-4a52-9acd-9c49b78c4360" />

### Marker Tools

#### 1. Set Marker Name

Sets the name of the marker that will be displayed on the [Timeline panel](#timeline-panel).

#### 2. Set Marker BPM

Sets the BPM of the marker.

> [!IMPORTANT]                  
> The BPM of the marker must be set to a valid value for a marker to be placed.


#### 3. Set Message

Sets the message of the marker that will be displayed initially when the marker becomes active. The information panel will not be displayed if the message is not set.


#### 4. Set Message Time

Sets the time duration that the message will be displayed for. The information panel will not be displayed if the message time is not set.

#### 5. Delete Current Mrk.

Deletes the current active marker that is displayed on the [Timeline Panel](#timeline-panel).

<img width="853" height="480" alt="2026-07-0821-05-32-ezgif com-video-to-gif-converter" src="https://github.com/user-attachments/assets/af9e7a03-d449-436c-8b60-554305f387ab" />
