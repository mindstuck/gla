/** Instrument family a track's button icon is drawn from. */
export type TrackKind = 'drums' | 'guitar' | 'bass' | 'violin' | 'piano' | 'other'

/**
 * Reactive mirror of one score track's control state.
 *
 * alphaTab's model is plain (non-reactive) data and keeps mute/solo/volume on
 * the synthesizer's channels rather than on the track itself, so the UI state
 * lives here and ScoreViewport pushes it back into the API on every intent.
 */
export interface TrackControl {
  /** Index into `score.tracks` — the handle for every alphaTab call. */
  index: number
  /** Human label, preferring the short name used on the score itself. */
  name: string
  kind: TrackKind
  muted: boolean
  solo: boolean
  /** Channel volume on alphaTab's 0–1 scale (`playbackInfo.volume / 16`). */
  volume: number
  /** Whether the track is part of the score's currently rendered view. */
  rendered: boolean
}
