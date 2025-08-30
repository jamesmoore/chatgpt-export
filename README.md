# ChatGPT Export²

<img width="1024" height="183" alt="ChatGPT Export Export" src="https://github.com/user-attachments/assets/3cf24bdf-df3e-48c8-97aa-c10e8d72bff0" />

## About

ChatGPT does not offer a clean, built-in way to save or print conversations for offline or archival use. While some browser extensions exist, they may pose security risks.

This tool enables you to extract and reformat your conversations from the official ChatGPT export ZIP file. You can export your data via [ChatGPT settings - https://chatgpt.com/#settings/DataControls](https://chatgpt.com/#settings/DataControls).

## Use cases
* **Create a readable archive** – Convert your ChatGPT conversations into clean Markdown or HTML files that can be opened offline, shared, or backed up.
* **Selective cleanup** – Keep a local copy of the chats you want to delete from the web interface, preserving only the chats you need.
* **Knowledge‑base ingestion** – Import your conversations into a wiki, personal knowledge base, or documentation system with minimal effort.
* **Account migration** – Export all conversations before closing your ChatGPT account, so you retain a viewable copy of your data in case you need it later.

## Features
* Convert ChatGPT exports into smaller, viewable: 
  * markdown files
  * html (Tailwind or Bootstrap formatted)
  * json files
* Process multiple exports in one sweep, detecting the latest version of each conversation.
* Include uploaded and generated image assets in the markdown.
* Transform web references into markdown footnotes.
* Include code blocks and canvas.
* Runs on Docker, Windows, Linux and MacOS.
* No usage limits or monetization.

## Quick‑Start (Bare metal)

1. Download the latest binary from the [Releases page](https://github.com/jamesmoore/chatgpt-export/releases).
2. (Optional) Add it to your `PATH`.
3. Unzip your ChatGPT export ZIP somewhere - **Important - keep an eye out for any ZIP errors**:
```sh
mkdir ~/chatgpt-export
unzip ~/Downloads/chatgpt_export.zip -d ~/chatgpt-export
```
4. Create a directory for the destination
```sh
mkdir ~/chatgpt-markdown
```
5. Run the tool
```sh
ChatGPTExport -s ~/chatgpt-export -d ~/chatgpt-markdown
```
6. Open `~/chatgpt-markdown` – you’ll see one file per conversation, plus an _complete.md for each with full history.

## Quick-Start (Docker)
1. Unzip your ChatGPT export ZIP somewhere - **Important - keep an eye out for any ZIP errors**:
```sh
mkdir ~/chatgpt-export
unzip ~/Downloads/chatgpt_export.zip -d ~/chatgpt-export
```
2. Create a directory for the destination
```sh
mkdir ~/chatgpt-markdown
```
3. Run the docker command (adapt the `-v ~/chatgpt-export` and `-v ~/chatgpt-markdown` parameters to the directories you have just created)
```sh
docker run --rm \
  -v ~/chatgpt-export:/source:ro \
  -v ~/chatgpt-markdown:/destination \
  ghcr.io/jamesmoore/chatgpt-export:latest \
  -s /source \
  -d /destination
```
4. Open `~/chatgpt-markdown` – you’ll see one file per conversation, plus an _complete.md for each with full history.

## Complete Usage

|Parameter|Optional|Usage|
|----|----|----|
|-?, -h, --help||Show help and usage information|
|--version||Show version information|
|-s, --source|Required|The source directory/directories containing the unzipped ChatGPT exported files.<br> Must contain at least one conversations.json, in the folder or one of its subfolders.<br>You can specify a parent directory containing multiple exports.<br>You can also specify multiple source directories (eg, -s dir1 -s dir2), and they will be processed in sequence.|
|-d, --destination|Required|The directory where markdown files and assets are to be created|
|-j, --json||Export to json files. [default: False]|
|-m, --markdown||Export to markdown files. [default: True]|
|--html||Export to html files. [default: False]|
|-hf, --htmlformat ||Specify format for html exports (Bootstrap or Tailwind). [default: Tailwind]|
|--validate||Validate the json against the known and expected schema. [default: False]|

## How it works
The source folder must contain a file named conversations.json, which holds all your conversations in JSON format. The conversations.json can be in a subfolder, and you can have multiple subfolders (eg, one for each export if you have created many).

Each conversation is converted into a standalone Markdown file in the destination folder. For each conversation, the following files may be created:

* A .json file containing just that conversation.
* A .md file named with a timestamp and the conversation title (eg, `<YYYY-MM-DDTHH-MM-SS> - <chat title>.md`) - this represents the most recent state of the conversation. The timestamp is the creation date of the conversation.
* Optionally, a _complete.md file - this extended version includes the full history of the conversation including edits, regenerations, and alternate messages in chronological order. If there are no edits or regenerations this will be absent.

Image assets (if present) are also extracted and copied to the destination folder.

## Tips
* Running this on a large export may create many files. It will also overwrite any existing files with the same name. Be sure to choose choose an empty destination directory for the first run.
* Keep a copy of your raw export ZIPs. In the future you may want to re‑run the tool to generate updated Markdown or a better format:
  * This program may be improved with new features in the future and you may want to rerun
  * Someone else may write a better one