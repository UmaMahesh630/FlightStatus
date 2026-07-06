# Prompt Log

This document records the AI prompts used during development and notes on how they were used.

## Prompt 1: Initial Spec Creation
* **Prompt**:
  ```text
  only create spec.md file from this document, this i am going to ask next steps
  create folder on Desktop folder FlightStatus
  first i need to commit spec.md file so first create that based on above document 
  ```
* **Actions Taken**:
  - Created the folder `FlightStatus` on the Desktop.
  - Extracted the requirements from the trial run PDF brief (domains, provider behaviors, normalization rules).
  - Drafted and wrote [spec.md](file:///Users/umamahesh/Desktop/FlightStatus/spec.md).
  - Initialized a git repository and made the initial commit.

## Prompt 2: GitHub Repository Setup
* **Prompt**:
  ```text
  I wanted to deploy that into github can you create FlightStatus folder in my github and commit
  ```
* **Actions Taken**:
  - Verified local Git config and SSH access.
  - Prompted for HTTPS URL as SSH authentication failed.
  - Linked local repository to `https://github.com/UmaMahesh630/FlightStatus.git` and successfully pushed `main`.

## Prompt 3: Solution Structure Setup
* **Prompt**:
  ```text
  Generate a professional full-stack solution structure for:
  Backend: .NET 8 Minimal API
  Frontend: Angular
  Testing: xUnit
  Documentation: README.md, spec.md, prompts.md, reflection.md
  Use a structure suitable for a senior .NET developer coding assignment.
  ```
* **Actions Taken**:
  - Created a C# Solution file `FlightStatus.sln`.
  - Created a Minimal Web API project `FlightStatus.Api` targeting .NET 8 (with `RollForward` configured for .NET 9 systems).
  - Created an xUnit test project `FlightStatus.Tests` referencing the API project.
  - Generated the Angular 19+ frontend app `flight-status-ui` using Angular CLI (defaults with routing and css).
  - Created `README.md` containing installation, run, and architecture notes.
  - Created this `prompts.md` file to track prompt usage.
