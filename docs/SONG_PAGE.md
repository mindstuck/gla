### Phase v1
Header - "GLA"
Main: Default score viewport (notes + tabs)
Main's footer: 
	Controls: stop, play, pause
	Song title & author

**Backend Side Infra**:
1. SongService (GET /songs/{id})
2. Basic scaffolding - DTO layer, db context, repo, service, connection string, configuration, etc

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
Introduction of user abstraction - with corresponding features added to the page, changes to backend and models.

