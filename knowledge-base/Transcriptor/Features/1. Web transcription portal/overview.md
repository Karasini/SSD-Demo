## Problem

People who need text from audio or video files often rely on command-line tools or paid online services. That workflow is slow and easy to get wrong: run commands by hand, track input and output files on disk, and open result files outside a single place.

Today there is no simple web app where a user can:

- upload an audio or video file in one place,
- see whether transcription is still running or already done,
- find earlier transcriptions in a list,
- read the finished text in a clear layout inside the same app.

## Goals

The main goal is a simple web-based transcription tool. A user uploads a file, starts transcription, follows progress, and reads the result in the browser without using the command line or juggling files on disk.

- Upload common audio and video files through a web portal and start transcription with minimal steps.
- See clear processing status while transcription runs (in progress, done, failed).
- Browse a list of past transcriptions and open any item to read the full text.
- Read transcription results in a layout that is easy to scan (readable text, clear title or file name, status visible).
- Export finished transcripts (for example download as a text file or copy to clipboard).
- Run more than one transcription at a time (upload and start a new job while others are still processing).
- Reduce dependence on manual CLI workflows and separate file management for day-to-day transcription needs.

**v1 product rules:** no user login; files up to **2 GB**; common audio and video formats (see `usecases.md`).

**Out of scope for this feature (v1):** user accounts and login, choosing or comparing transcription engines, billing, advanced editing of transcripts (collaboration, timestamps, speaker labels), and mobile-native apps.
