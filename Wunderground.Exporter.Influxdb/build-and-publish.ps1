$dockerUsername = "sakrut"
$dockerImageName = "wunderground.exporter.influxdb"
$dockerTag = "latest"


docker build -t "${dockerUsername}/${dockerImageName}:${dockerTag}" .

docker login --username $dockerUsername


docker push "${dockerUsername}/${dockerImageName}:${dockerTag}"


docker logout
