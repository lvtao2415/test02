def msBuildSolutionFileDir = "APIProject"
def msBuildSolutionFile = "test02.sln"
def msBuildList = [
    [
        workDir: 'APIProject',
        file: 'test02.sln'
    ],
    [
        workDir: 'APIProject/testDto',
        file: 'testDto.csproj'
    ]
]
def publishProjectList =    [
        [ 'APIProject/test02', 'true', ['TestProject'], "Test.API"],
        [ 'APIProject/testDto', 'true', ['TestProject'], "Test.Dto"]
]

pipeline {
    agent any
    options {
        timeout(time: 60, unit: 'MINUTES')
    }

    parameters {
        string(name: 'MailForm', defaultValue: '12@34', description: '')
    }

    stages {
        stage('PrepareEnv') {
            steps {
                script {
                    fileOperations([
                        folderCreateOperation('tools'),
                        fileDownloadOperation(userName: '', password: '', targetFileName: 'README.md', targetLocation: "./tools", url: "https://github.com/lvtao2415/hello-world/blob/master/README.md")
                    ])
                }
            }
        }
        stage('Build') {
            when {
                equals expected: 'SUCCESS', actual: currentBuild.currentResult
            }
            steps {
                script {
                    def versionYear = new java.text.SimpleDateFormat("yy").format(new Date()).toInteger()
                    def dayOfYear = new java.text.SimpleDateFormat("D").format(new Date()).toInteger()
                    def versionWeekOfYear = new java.text.SimpleDateFormat("w").format(new Date()).toInteger()
                    env.VERSION_NEW = "${versionYear}.${dayOfYear}.${versionWeekOfYear}.${currentBuild.number}"
                    echo "set in enviroment env.VERSION_NEW: ${env.VERSION_NEW}"
                }
                script {
                    for (build in msBuildList) {
                        bat """
                        cd ${build.workDir}
                        set MSBUILDDEBUGPATH="./buildlogs/"
                        dotnet msbuild /m /verbosity:normal /t:Restore /t:Rebuild /p:Configuration=Release /p:DefineConstants=\"KONGREGISTER\" /p:PublishDir=./bin/PublishTemp/ ${build.file}
                        """  
                    }
                }
            }
        }   
        stage('UnitTest'){
            when {
                equals expected: 'SUCCESS', actual: currentBuild.currentResult
            }
            steps {
                fileOperations([
                    fileDeleteOperation(includes: "**/unittest-result*", excludes: "")
                ])

                script {
                    bat """
                    cd ${msBuildSolutionFileDir}
                    dotnet test --no-build ${msBuildSolutionFile} /m /p:Configuration=Release /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura --logger \"trx;LogFileName=unittest-result.trx\" || exit 0 
                    """
                }
            }
        }
        stage('Publish') {
            when {
                equals expected: 'SUCCESS', actual: currentBuild.currentResult
            }
            steps {
                script {
                    // clean all previously publish folder
                    for (proj in publishProjectList) {
                        fileOperations([
                            folderDeleteOperation(folderPath: "./${proj[0]}/bin/Publish")
                        ])
                    }
                    // publish solution for use parallel build&publish
                    for (build in msBuildList) {
                        bat """
                        cd ${build.workDir}
                        dotnet publish --no-build /m /verbosity:minimal /p:Configuration=Release /p:DefineConstants=\"KONGREGISTER\" /p:PublishDir=bin/PublishTemp/ ${build.file} || exit 0
                        """
                    }
                    // clean Development files, prepare release files
                    for (proj in publishProjectList) {
                        fileOperations([
                            folderRenameOperation(source: "${proj[0]}/bin/PublishTemp" , destination: "${proj[0]}/bin/Publish"),
                            // fileDeleteOperation(includes: "${proj[0]}/bin/Publish/appsettings.Development.json", excludes: "")
                        ])
                    }
                }
            }
        }
        stage('Pacakge') {
            when {
                equals expected: 'SUCCESS', actual: currentBuild.currentResult
            }
            parallel {
                stage('Nuget') {
                    steps {
                        script{
                            for (proj in publishProjectList) {
                                // check is need buikd nuget pacakge
                                if(proj[1] != 'true') {
                                    continue
                                }
                                fileOperations([
                                    folderDeleteOperation(folderPath: "./${proj[0]}/bin/nupkg")
                                ])
                                echo "Will show ${params.MailForm}"
                                echo "Will show ${params.MailForm.tokenize('@')[1]}"
                                
                                echo "${WORKSPACE}"
                                echo "${getWindowsStylePath("${proj[0]}/bin/nupkg/*.nupkg")}"
                                bat "\"${WORKSPACE}\\tools\\octo.exe\" pack --id=${proj[3]} --format=NuPkg --version=${env.VERSION_NEW} --outFolder=\"${proj[0]}/bin/nupkg\" --basePath=\"${proj[0]}/bin/Publish\" --author=\"${BUILD_TAG}\" --description=\"BuildNumber: ${BUILD_NUMBER}  GitBranch: ${GIT_BRANCH}  GitCommit: ${GIT_COMMIT}\""
                            }
                        }
                    }
                }
            }
        }


    }
    post {
        success {
            script {
                echo "currentResult: ${currentBuild.currentResult} result: ${currentBuild.result}"
            }
        }
        always {
            script {
                def buildResult = currentBuild.currentResult
                def prevBuild = currentBuild.previousBuild
                def prevBuildResult = prevBuild == null ? 'N/A' : prevBuild.result
                def subject = "${buildResult} : ${env.JOB_NAME} #${env.BUILD_NUMBER}"
                def body = """<p>EXECUTED: test <b>\'${env.JOB_NAME}: #${env.BUILD_NUMBER}\' ${buildResult}
                                  </b></p><p>View console output at "<a href="${env.BUILD_URL}">
                                  ${env.JOB_NAME}: #${env.BUILD_NUMBER}</a>"</p> """
                def recipientProviders = [
                    [$class: 'DevelopersRecipientProvider'],
                    [$class: 'RequesterRecipientProvider'],
                    [$class: 'CulpritsRecipientProvider']
                ]
                def prevBuildHasError = prevBuildResult.equals('FAILURE') || prevBuildResult.equals('UNSTABLE')
                
                def emailextOptions = [
                    subject: subject,
                    body: body,
                    recipientProviders: recipientProviders
                ]
                emailextOptions.to = "${params.MailForm}"
                emailext emailextOptions
            }
        }
    }



}