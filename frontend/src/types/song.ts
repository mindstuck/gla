/**
 * Mirrors SongDto returned by the backend (backend/Dtos/SongDto.cs).
 */
export interface Song {
  id: number
  title: string
  author: string
  filePath: string
}
