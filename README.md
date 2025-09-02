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

|Parameter|Optional?|Usage|Default|
|----|----|----|----|
|`-?`<br>`-h`<br>`--help`||Show help and usage information||
|`--version`||Show version information||
|`-s`<br>`--source`|Required|The source directory/directories containing the unzipped ChatGPT exported files.<br>Must contain at least one conversations.json, in the folder or one of its subfolders.<br>You can specify a parent directory containing multiple exports.<br>You can also specify multiple source directories (eg, -s dir1 -s dir2)||
|`-d`<br>`--destination`|Required|The directory where markdown files and assets are to be created||
|`-e`<br>`--export`||Export mode (`latest` or `complete`).|`latest`|
|`-j`<br>`--json`||Export to json files (`true` or `false`).|`false`|
|`-m`<br>`--markdown`||Export to markdown files (`true` or `false`).|`true`|
|`--html`||Export to html files (`true` or `false`).|`true`|
|`-hf`<br>`--htmlformat`||Specify format for html exports (`bootstrap` or `tailwind`).|`tailwind`|
|`--validate`||Validate the json against the known and expected schema.|`false`|

## How it works
The source folder must contain a file named conversations.json, which holds all your conversations in JSON format. The conversations.json can be in a subfolder, and you can have multiple subfolders (eg, one for each export if you have created many).

Each conversation is converted into one of more files in the destination folder. Depending on the parameters passed in, json, markdown and html files may be created.

The files will be named with a timestamp and the conversation title (eg, `<YYYY-MM-DDTHH-MM-SS> - <chat title>.md`). The timestamp is the creation date of the conversation.

For markdown and html exports, any image assets are also extracted and copied to the destination folder.

### Export modes
There are two export modes - Latest and Complete. This is to handle conversations that have multiple branches. In ChatGPT If you click "Try again..." or go back and edit one of your previous messages, this causes the conversation to start a new branch. The old branch is hidden and the conversation continues on the latest branch.

Latest is the recommended mode, as it will produce an export that contains the latest instance of the conversation. Complete mode will include all the old hidden branches, but not in any specific order.

## Tips
* Running this on a large export may create many files. It will also overwrite any existing files with the same name. Be sure to choose choose an empty destination directory for the first run.
* Keep a copy of your raw export ZIPs. In the future you may want to re‑run the tool to generate updated Markdown or a better format:
  * This program may be improved with new features in the future and you may want to rerun
  * Someone else may write a better one
