## (*Guitar Learning App*)
#portfolio 

The app to make learning guitar songs more fun and joyful. Find or upload tabs - and get difficulty, analysis and practicing recommendations

## The Action
You want to learn how to play the song on guitar.
Your typical actions:
1. Search for "paranoid android guitar tab" on youtube
2. Get some relevant results
3. Open one of the videos
4. Start following along and practice
5. The end

## The Problem
- There are no relevant videos on Youtube
- The tabs are bad
- The song ends up being too difficult for you after a few days of practicing
- You stumble across the playing technique you can't master
- You lack the sense of progress -> learn too slow and/or give up
- You lack the structure of the learning process -> learn too slow and/or give up
-  

## The Solution
**MVP**
You open a web app (desktop/mobile later maybe)

Your input:
> Upload `.gp/.gp5/.gpx/.musicxml/.mid`

Output:
1. Interactive tab
2. Song divided into fragments
3. Difficulty per fragment
4. Overall difficulty
5. Detected techniques
6. Recommended learning order
7. "Practice this fragment" mode
8. Mark fragment as learned
9. Progress tracking


**Features after MVP proves the the actual value**:
- You find the song in the app -> app finds notes and/or tabs, potentially different versions if exist from all available sources. [Are there sources for this kind of data?]
- Analyze tabs/notes and evaluate difficulty grading for individual fragments and the whole song (fragments can have different color assigned based on the difficulty & be responsive to community feedback in the future)
- Listen to user's attempt and detect successfully executed fragment
- Link music platform account and ask the app to do various operations with the songs/playlists:
	- find notes/evaluate difficulty
	- check notes availability/evaluate difficulty for the whole playlist
	- based on playlist, find similar songs specifically with existing notes 
- Community notes
- Built-in tuner
- More instruments
- Tracker (number of learned, in progress, etc)
- AI powered tab creation



## Example flows

> **MVP**

Upload Paranoid Android.gp
             ↓
      Parse the score
             ↓
       Split into phrases
             ↓
   Extract musical features
             ↓
      Difficulty model
             ↓
      Learning planner
             ↓
       Practice UI

---

> Playlist analysis

Spotify playlist
       ↓
80 songs
       ↓
find available arrangements
       ↓
difficulty analysis
       ↓
filter by user's ability
       ↓
      17 songs
       ↓
"These are within your current range"

---

> Attempt evaluation

TAB
 ↓
Expected notes
 ↓
Microphone / audio interface
 ↓
Pitch detection
 ↓
Onset detection
 ↓
Expected vs actual
 ↓
Fragment completion

---

> AI Powered Tab Creation

Song audio
   ↓
AI transcription
   ↓
candidate tab
   ↓
human correction
   ↓
your analysis