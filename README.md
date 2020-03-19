# Webhook Runner for Github

React to [GitHub webhook events](https://developer.github.com/webhooks/) and run custom scripts on your server. This is useful if you want to setup simple continious integration and\or deployment.

## How does it work?

It listens on the configured port (http://localhost:5000 by default) and have only one API endpoint `POST /api/webhook`. When it receives payload form GitHub, it will loop through predefined set of rules (defined in appsettins.json) and will execute command line from `Execute` property if `ref` value from payload matches `Match` value from the rule.
If you [configured](https://developer.github.com/webhooks/securing/#validating-payloads-from-github) `Secret` for the webhook, it will compare value for the `X-Hub-Signature` header with the WebHookSecret environment valiable.

## How to run it?
You can download self-contained application from for your platform:
- Linux: 
- Windows:

Copy it to the web-server, extract, and run.

You can expose it to the outside world but I would recommend setting up nginx with SSL certificat (for example from Let's Encrypt) as a reverse-proxy in front of the WebHook Runner

If you decided to run it without nginx, do not forget to add rule in  the firewall (I hope you are using it) to allow access on the port 5000.

## How to configure webhook in GitHub

Configure webhook in GitHub repository:

- Set `Payload URL` to the publicly visible URL of the Webhook Runner (e.g. http://example.com:5000/api/webhook)
- Set `Content type` to `application/json`
- `SSL verification` based on your server confgig (I would strongly recomed setting up nginx with SSL certifiacte in front of the runner) 
- Set `Just the push event.` because currently this is the only supported event type (GitHub will get `BadRequet` response for any other event types)
- `Active` should be enabled

## How to configure webhook Webhook Runner

You can see the example in appconfig.json, it has two rules defined:

The first one will react to `push` to the `master` branch (`refs/heads/master` match) and will execute `build_and_restart.sh` script (it should be in the same root folder).

The second one will do run `start_dev.sh` script whenever something is pushed to `dev` branch.

## How to build and contribute

- Clone repo
- Install .NET Core SDK 3.1
- Develop
- Create PR

## Examples