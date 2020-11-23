def msBuildSolutionFileDir = "mytest02"
def msBuildSolutionFile = "test02.sln"
def msBuildList = [
    [
        workDir: 'mytest02',
        file: 'test02.sln'
    ],
    [
        workDir: 'mytest02/testDto',
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
                    echo "${versionYear}-${dayOfYear}-${versionWeekOfYear}"
                    echo "New version computed: ${versionYear}"
                }
                
            }
        }   



    }
}