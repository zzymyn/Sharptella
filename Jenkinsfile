pipeline {
  agent none

  parameters {
    booleanParam(name: 'BUILD_WINDOWS', defaultValue: false, description: 'Build Windows')
    booleanParam(name: 'BUILD_LINUX', defaultValue: false, description: 'Build Linux')
    booleanParam(name: 'BUILD_MACOS', defaultValue: false, description: 'Build macOS')
  }

  environment {
    DOTNET_PROJECT = 'Sharptella.Gui/Sharptella.Gui.csproj'

    WINDOWS_OUTPUT = 'Publish.Windows'
    LINUX_OUTPUT = 'Publish.Linux'
    MACOS_OUTPUT = 'Publish.macOS'

    WINDOWS_ZIP = 'Sharptella.Windows.zip'
    LINUX_ZIP = 'Sharptella.Linux.zip'
    MACOS_ZIP = 'Sharptella.macOS.zip'
  }

  options {
    timestamps()
    skipDefaultCheckout()
  }

  stages {
    stage('Build') {
      parallel {
        stage('Build Windows') {
          agent {
            node {
              label 'Windows'
            }
          }
          when {
            expression {
              return params.BUILD_WINDOWS
            }
          }
          steps {
            checkout scm
            powershell '''
                $ErrorActionPreference = "Stop"

                Remove-Item -Recurse -Force "${env:WINDOWS_OUTPUT}" -ErrorAction SilentlyContinue
                Remove-Item -Force "${env:WINDOWS_ZIP}" -ErrorAction SilentlyContinue
                New-Item -ItemType Directory -Path "${env:WINDOWS_OUTPUT}" -Force | Out-Null

                dotnet restore "${env:DOTNET_PROJECT}"
                dotnet publish "${env:DOTNET_PROJECT}" -c Release -r win-x64 --self-contained true -o "${env:WINDOWS_OUTPUT}" /p:DebugType=none /p:DebugSymbols=false /p:CopyOutputSymbolsToPublishDirectory=false

                Compress-Archive -Path "${env:WINDOWS_OUTPUT}/*" -DestinationPath "${env:WINDOWS_ZIP}" -Force
            '''.stripIndent()
            archiveArtifacts artifacts: "${env.WINDOWS_ZIP}", fingerprint: true
          }
        }

        stage('Build Linux') {
          agent {
            node {
              label 'Linux'
            }
          }
          when {
            expression {
              return params.BUILD_LINUX
            }
          }
          steps {
            checkout scm
            sh '''
                #!/usr/bin/env bash
                set -ex

                rm -rf "${LINUX_OUTPUT}"*
                rm -f "${LINUX_ZIP}"
                mkdir -p "${LINUX_OUTPUT}"

                dotnet restore "${DOTNET_PROJECT}"
                dotnet publish "${DOTNET_PROJECT}" -c Release -r linux-x64 --self-contained true -o "${LINUX_OUTPUT}" /p:DebugType=none /p:DebugSymbols=false /p:CopyOutputSymbolsToPublishDirectory=false

                (
                  cd "${LINUX_OUTPUT}"
                  zip -r "../${LINUX_ZIP}" .
                )
            '''.stripIndent()
            archiveArtifacts artifacts: "${env.LINUX_ZIP}", fingerprint: true
          }
        }

        stage('Build macOS') {
          agent {
            node {
              label 'macOS'
            }
          }
          when {
            expression {
              return params.BUILD_MACOS
            }
          }
          steps {
            checkout scm
            sh '''
                #!/usr/bin/env bash
                set -ex

                rm -rf "${MACOS_OUTPUT}"*
                rm -f "${MACOS_ZIP}"
                mkdir -p "${MACOS_OUTPUT}"

                dotnet restore "${DOTNET_PROJECT}"
                dotnet publish "${DOTNET_PROJECT}" -c Release -r osx-x64 --self-contained true -o "${MACOS_OUTPUT}" /p:DebugType=none /p:DebugSymbols=false /p:CopyOutputSymbolsToPublishDirectory=false

                ditto -c -k --sequesterRsrc --keepParent "${MACOS_OUTPUT}" "${MACOS_ZIP}"
            '''.stripIndent()
            archiveArtifacts artifacts: "${env.MACOS_ZIP}", fingerprint: true
          }
        }
      }
    }
  }
}