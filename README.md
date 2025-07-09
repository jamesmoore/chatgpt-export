# ChatGPT Export²

## About

ChatGPT does not offer a clean, built-in way to save or print conversations for offline or archival use. While some browser extensions exist, they may pose security risks.

This tool enables you to extract and reformat your conversations from the official ChatGPT export ZIP file. You can export your data via [ChatGPT settings](https://chatgpt.com/#settings/DataControls).

### Process

1. Export your data from the ChatGPT settings page.
2. Download the ZIP file from the email you receive.
3. **Important:** Verify that the ZIP file is valid. Some ChatGPT exports may be truncated or corrupted.
4. Unpack the ZIP file to a new folder.
5. Run this tool using that folder as the source, and specify another folder as the destination.

## Usage

```
ChatGPTExport.exe --help
Description:
  ChatGPT export reformatter

Usage:
  ChatGPTExport [options]

Options:
  -?, -h, --help                Show help and usage information
  --version                     Show version information
  -s, --source (REQUIRED)       The source directory containing the unzipped ChatGTP exported files.
                                Must contain a conversations.json.
                                You can specify multiple source directories (eg, -s dir1 -s dir2), and they will be processed in sequence.
  -d, --destination (REQUIRED)  The the destination directory where markdown files and assets are to be created.

```

## Running with docker

Change the volume paths according to your filesystem. Keep the source as read-only:

```sh
docker run --rm \
  -v /mnt/storage/docker/chatgpt-export:/source:ro \
  -v /mnt/storage/docker/chatgpt-export-export:/destination \
  ghcr.io/jamesmoore/chatgpt-export:latest \
  -s /source \
  -d /destination
```

## How it works
The source folder (unzipped export) must contain a file named conversations.json, which holds all your conversations in JSON format.

Each conversation is converted into a standalone Markdown file in the destination folder. For each conversation, the following files may be created:

* A .json file containing just that conversation.
* A .md file named with a timestamp and the conversation title - this represents the most recent state of the conversation.
* Optionally, a _complete.md file - this extended version includes all edits, regenerations, and alternate messages in chronological order. If there are no edits or regenerations this will be absent.

Image assets (if present) are also extracted and copied to the destination folder.