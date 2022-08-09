# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased

## 1.0.3 - 2018-11-19
### Added
- Output current date and time zone

### Fixed
- Only prompt for key entry if running in debug

## 1.0.2 - 2018-11-15
### Fixed
- Code cleanup thanks to Roslynator

## 1.0.1 - 2018-11-07
### Fixed
- Dockerfile to not use Web image and conventions (port, shared directory)

## 1.0.0 - 2018-11-07
### Added
- Connecton to Envisionware MySQL database to extract data
- SQL Server table to contain Envisionware data (straight load)
- SQL Server tables to aggregate data hourly and daily
- Twelve-month trailing maximum for each summary record for estimating usage level
- Docker build infrastructure
