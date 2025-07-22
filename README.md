# File Transfer System (FileTransferSender ↔ FileTransferReceiver ↔ FileTransferUI)

A high-speed, chunk-based file transfer system built with **ASP.NET 8**, designed to transfer files from one machine to another as fast and efficiently as possible. It includes a web UI for controlling and monitoring the upload process.

## 📁 Project Structure

FileTransfer/
│   
├── FileTransferSender/ → ASP.NET Web API for sending files   
├── FileTransferReceiver/ → ASP.NET Web API for receiving and assembling files   
├── FileTransferUI/ → Web interface to trigger uploads and track progress   
├── README.md → This file   
├── .gitignore → Ignore compiled and irrelevant files   
└── FileTransfer.sln (optional, includes all three projects)   


## ⚙️ Purpose

- 🚀 Maximize transfer speed using chunked uploads
- 📦 File is split into chunks (e.g. 5MB each)
- 🔁 Chunks are uploaded asynchronously with configurable concurrency
- 🎥 UI includes live progress bar + video synced with real-time transfer speed
- 🔓 Security is not a priority — raw performance is the focus

## 🛠 How It Works

1. **FileTransferSender**
   - Reads files from `FileTransferSender/kargo/`
   - Splits the file into chunks (`chunkSize` can be configured)
   - Uploads chunks asynchronously (`concurrency` level can be set)

2. **FileTransferReceiver**
   - Receives chunks via HTTP POST
   - Reassembles them in order once all are received
   - Saves the final file on disk

3. **FileTransferUI**
   - Allows user to select and send a file
   - Shows real-time upload progress
   - Plays a video whose speed is dynamically synced with average upload speed so it ends exactly when transfer completes

## 🧪 Getting Started (Dev Mode)

> Prerequisite: [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

```bash
# Clone the repo
git clone https://github.com/your-username/FileTransfer.git
cd FileTransfer

# Run Receiver
cd FileTransferReceiver
dotnet run

# In a new terminal

🔧 Configuration
chunkSize and concurrency can be modified in FileTransferSender (e.g., in appsettings.json or directly in code)

Receiver IP/port must be defined in SenderController or relevant config

UI communicates with both APIs assuming default ports (e.g. http://localhost:5000, http://localhost:5001)

🧠 Notes
Sender, Receiver and UI are completely decoupled

APIs can be hosted on separate machines

Designed for use in local networks or closed environments

Works both with dotnet run and after publishing (e.g. dotnet publish -c Release)

📄 License
MIT — do whatever you want, but don't blame me if it breaks 😄
