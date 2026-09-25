import type { Song } from '../types/song'

/**
 * Same-origin API base: in development Vite proxies /api/* to the ASP.NET Core
 * backend (see vite.config.ts), so no CORS or base-URL configuration is needed.
 */
const API_BASE = '/api'

export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

async function getJson(path: string): Promise<unknown> {
  let response: Response
  try {
    response = await fetch(`${API_BASE}${path}`, {
      headers: { Accept: 'application/json' },
    })
  } catch {
    throw new ApiError(0, `GET ${path} failed: network error`)
  }

  if (!response.ok) {
    throw new ApiError(
      response.status,
      `GET ${path} responded ${response.status} ${response.statusText}`,
    )
  }

  return (await response.json()) as unknown
}

/** Returns all songs, or throws ApiError. */
export async function fetchSongs(): Promise<Song[]> {
  return (await getJson('/songs')) as Song[]
}

/**
 * Returns the song, null when it does not exist (404),
 * or throws ApiError for any other failure.
 */
export async function fetchSongById(id: number): Promise<Song | null> {
  try {
    return (await getJson(`/songs/${id}`)) as Song
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) {
      return null
    }
    throw error
  }
}
