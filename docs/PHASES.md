### Phase v1
Header - "GLA"
Main: Default score viewport (notes + tabs)
Main's footer: 
	Controls: stop, play, pause
	Song title & author

**Backend Side Infra**:
1. SongService (GET /songs/{id})
2. Basic scaffolding - DTO layer, db context, repo, service, CORS, connection string, configuration, etc

**Models**:
*SongEntity*:
	Id - int
	CreatedAt - datetime2
	UpdatedAt - datetime2
	Title - nvarchar
	Author - nvarchar
	FilePath - nvarchar (pointing to location on orange pi, e.g "/BlackHoleSun.gp4" to be later accessed by https://gla.orbitalgarden.net/songs/files/user123/BlackHoleSun.gp4)

*SongDTO*:
	Id
	Title,
	Author,
	FilePath

**Client Side infra**: 
- ts models for API objects
- song API service

### Phase v2
1. **Score track controls**
	The score can contain multiple tracks (guitar, drums, bass, etc)
	alphaTab's API does feature the functionality for controlling this already.
	
	**UI**
	Two different options for mobile and desktop view.
	Keep the styles and color aligned with the page.
	
	*Desktop*: absolutely positioned circle-shaped buttons, aligned on top of the left top corner of the score page vertically. Clicking the circle mutes/unmutes the track. Hovering onto it for a few milliseconds (~400) opens a small popup featuring controls for this individual track: a volume slider, solo button (S), and an open button to render the selected track.
	
	*Mobile*: the controls with default positioning should appear at the bottom instead. Instead of the popup, show the controls indifferently on the right side of the corresponding track's button. 
	
	*Notes*:
	- Each button should look the same visually as on desktop.
	- Each button should have suitable icon (e.g. drums for drums, guitar for guitar, ..., the fallback one)
	- Muted track's button should be visibly grayed out
	- Currently rendered track's button should be visibly highlighted
	- On desktop, tracks buttons can slightly overlap with the beginning of the score (treble clef), in order to achieve optimal not too small size of the buttons. 
1. **Tracks list**
	The lobby page for the tracks. Displays rows with info about the song, navigates to the song page.
	This page should be accessible by route /songs. The home / route should now also redirect to this page. Keep the styles aligned with the individual song page. 

### Phase v3
Introduction of user abstraction - with corresponding features added to the page, changes to backend and models. 

