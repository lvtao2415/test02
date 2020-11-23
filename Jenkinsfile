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

pipeline {
    agent any
    options {
        timeout(time: 60, unit: 'MINUTES')
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
                    dotnet test --no-build ${msBuildSolutionFile} /m /p:Configuration=Release /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Include=\\\"${unitTestCoverIncludeFilter}\\\" /p:Exclude=\\\"${unitTestCoverExcludeFilter}\\\" --logger \"trx;LogFileName=unittest-result.trx\" || exit 0 
                    """
                }
                echo "Publish unit test report"
                script {
                    echo "Publish nunit report"
                    xunit (
                        thresholds: [ failed(failureThreshold: '0') ],
                        tools:[ MSTest(pattern:"**/unittest-result*.trx", skipNoTestFiles:true, failIfNotNew:false, deleteOutputFiles:false, stopProcessingIfError:false) ]
                    )
                }           
                script {
                    echo "Publish nunit coverage report(cobertura)"
                    cobertura coberturaReportFile:"**/coverage.cobertura.xml"
                }
            }
        }







    }
}