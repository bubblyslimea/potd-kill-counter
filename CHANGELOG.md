# little changes

## 0.1.5.0 · a moment for the victory

- the defeated boss’s name becomes the headline, with **DEFEATED** and a green check beneath it
- confirmed victories appear after combat ends, fade in gently and stay for four seconds
- confirmed passage status remains visible, then OPEN returns after the victory moment
- a reset, wipe, add death or duplicate event cannot start or replay the card
- long names use two lines and fitted, baked fonts
- tracker wording stays neutral, the completion card now says **floor 200 complete**
- preview includes victory, a two-line name, pending passage and the post-victory state

## 0.1.4.0 · a little hello before the boss

- each boss floor shows its name directly below **BOSS**
- the HUD disappears when the client enters combat on a boss floor
- it returns after combat ends, with your visibility preference still respected
- normal-floor fights keep their kills-left counter
- preview includes before, during and after a boss encounter

boss names cover floors 10–190, floor 200 keeps its completion card

## 0.1.3.0 · a count from the first floor

- kills left stay visible when incoming Flight is unavailable or unconfirmed
- floor 1 keeps its estimate above **FIRST FLOOR: Flight Unavailable**, including after a late attachment
- incomplete counts use a wider range with **kills left · partial**
- later starting floors cover both possible Flight targets until the effect is known
- confirmed passage activation still wins, unavailable tracking still pauses estimates
- preview includes fresh floor 1, late floor 1 and a later set start

the HUD layout, counting hooks and Flight carry timing are unchanged

## 0.1.2.0 · read the state, keep moving

- the approved Watch-inspired HUD replaces the two-column layout
- crisp, bold kills left take the main spot, a confirmed passage shows `OPEN` with a checkmark
- explicit `ACTIVE`, `INACTIVE` and `UNKNOWN` Flight badges stay in one place
- next-floor Flight and boss notices have their own reserved space
- floor 1 gets `FIRST FLOOR:` / `Flight Unavailable`, floor 199 points to completion
- observed counts and full ranges move to **run details** in `/potd`
- 15 native preview examples include the approved designs and uncertain states
- saved placement, lock and opacity stay yours, `/potdcount` still works

counting and Flight timing are unchanged, the new native appearance still needs its in-game review

## 0.1.1.0 · a little more room to breathe

- `/potd` opens settings now too, with the same `preview` and `toggle` options as `/potdcount`
- the estimated kills left gets the largest text, with floor and observed kills above it
- Flight this floor and Flight next floor have their own columns
- full ranges and explanations tuck into **details**, closed by default
- uncertainty stays visible, and a confirmed open passage takes the main spot
- settings are quieter too, with optional explanations and a more complete simulation

counting, Flight transitions and floor ranges keep the same behavior
the first alpha has a positive live report, the redesigned native overlay still needs its own in-game look

## 0.1.0.0 · first alpha

a little counter for the climb

- counts observed enemy deaths and starts fresh on each floor
- keeps Flight here separate from Flight queued for the next floor
- shows a rough range until the game confirms the passage is open
- keeps incomplete counts and unknown Flight clearly marked
- includes a moveable overlay, click-through lock and a simulation to try outside PotD

this is the first testing release, native game events and Flight timing still need a live PotD check
