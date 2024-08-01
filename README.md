# OuraRing

Oura Ring client, sample web hook Azure function and sample web hook subscription client.

## Oura Ring client

For simple data querying you need to pass single `oura_personal_token` parameter to the `OuraRingClient` constructor.
See [https://github.com/ivan-shaporov/TrayHeartRate](https://github.com/ivan-shaporov/TrayHeartRate) repo for a usage example.


## Web hooks

For managing web hooks you need to pass `oura_client_id` and `oura_client_secret` to the constructor.
Web hooks are not necessary to query the data from Oura. You can use the Oura API to get the data. However, web hooks are useful if you want to get some notifications. 
To have web hooks working:

1. Create an Oura application to get the client id and client secret. You can do this at https://cloud.ouraring.com/oauth/applications.
1. Copy `local.settings.template.json` as `local.settings.json` and fill in `oura_client_secret`.
1. Run the web hook server to receive the data from Oura. You can do this by running the Azure function in the OuraRingWebHook folder.
1. Open this url in a web browser:
https://cloud.ouraring.com/oauth/authorize?response_type=token&client_id=< client id >&redirect_uri=http%3A%2F%2F< host[:port] >%2Fapi%2FOAuth&scope=email+personal+daily+heartrate+workout+tag+session+spo2&state=somestate

    where:
    < client id > is your Oura Application client id.
    '< host[:port] >' is public address of the Azure function without `http://`.
    You can run it locally but need to setup TCP port forwarding and make sure that your server is reachable by public IP.

    You will be asked to authorize the application for your own Oura user.
    Then the web hook will be called with a token and other data in the query string but they are not     necessary for subscriptions to work.

1. See OuraControl project in this repository to see how to subscribe to the web hooks

### Configuring OuraControl user secrets for web hooks

Either use Visual Studio "Manage User secrets" feature or open. `%AppData%\Microsoft\UserSecrets\63b62c45-4c1d-482b-9d60-50ed4709017b\secrets.json` and add the following:

```json
{
  "oura_client_id": "client id of your Oura application",
  "oura_client_secret": "client secret of your Oura application"
}
```
