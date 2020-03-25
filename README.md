# Webhook Runner for Github

React to [GitHub webhook events](https://developer.github.com/webhooks/) and run custom scripts on your server. This is useful if you want to setup simple continious integration and\or deployment.

![.NET Core](https://github.com/drussilla/git-webhook-server/workflows/.NET%20Core/badge.svg)

## Description

It listens on the configured port (http://localhost:5000 by default) and have only one API endpoint `POST /api/webhook`. When it receives payload form GitHub, it will loop through defined set of rules (defined in `appsettins.json`) and will execute command line from `Execute` property if `ref` value from payload matches `Ref` value from the rule and `repository.url` from the payload matches `RepositoryUrl` from the rule.
If you [configured](https://developer.github.com/webhooks/securing/#validating-payloads-from-github) `Secret` for the webhook, it will compare value for the `X-Hub-Signature` header with the `WebHookSecret` environment valiable (can also be set in appsettings.config, but I would recomend keeping it as an envirunment variable).

## Run

You can download self-contained application for your platform:

- Linux-x64: https://github.com/drussilla/git-webhook-server/releases/download/0.4/linux-x64-v0.4.tar.gz
- Windows-x64: https://github.com/drussilla/git-webhook-server/releases/download/0.4/win-x64-v0.4.zip

Download:

```bash
wget https://github.com/drussilla/git-webhook-server/releases/download/0.4/linux-x64-v0.4.tar.gz
```

Make it executable:

```bash
cd linux-x64
chmod +x git-webhook-server
```

Create file `nano appsettings.json` with rules:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "Rules": [
    {
      "Name": "master git-webhook-server",
      "Ref": "refs/heads/master",
      "RepositoryUrl": "https://github.com/drussilla/git-webhook-server",
      "Execute": "run_in_tmux.sh"
    }
  ]
}
```

Node: Do not forget to replace valus for `RepositoryUrl` and `Execute` properties.

Run:

```bash
./git-webhook-server --urls http://0.0.0.0:5000
```

You can expose it to the outside world but I would recommend setting up `nginx` with `SSL` certificat (for example from Let's Encrypt) as a reverse-proxy in front of the WebHook Runner

If you decided to run it without nginx, do not forget to add rule in the `firewall` (I hope you are using it) to allow access on the port 5000.

## Running as a deamon

There are few ways to deamonize this copnsole app (actuially you can do the same with any console app)

### Using Tmux

Install [tmux](https://github.com/tmux/tmux/wiki) - a terminal multiplexer:

```bash
sudo apt-get install tmux
```

Run new tmux session:

```bash
tmux
```

Run console app inside tmux session:

```bash
cd linux-x64
./git-webhook-server --urls http://0.0.0.0:5000
```

Detach from tmux session by pressing `Ctrl+B Ctrl+D`. Now you can exit your ssh session and the process will kipp running. When you login back, you can attach to the running tmux session by typing:

```bash
tmux attach
```

### Using supervisor

If you do not want to run it manually everytime you reboot your machine or when the process is crashed, you can use [surepvisor](http://supervisord.org/) - system that allows its users to monitor and control a number of processes on UNIX-like operating systems.

Install

```bash
sudo apt-get install supervisor
```

Create config file for your application

```bash
sudo nano /etc/supervisor/conf.d/git-webhook-server.conf
```

With the following content:

```ini
[program:git-webhook-server]
command=su -c "/home/<username>/linux-x64/git-webhook-server --urls http://0.0.0.0:5000" <username>
directory=/home/<username>/linux-x64
autorestart=true
autostart=true
stdout_logfile=/home/<username>/linux-x64/out.log
stderr_logfile=/home/<username>/linux-x64/err.log
```

**Note:**: Replace *\<username\>* with the actual username.

Reload supoervisor:

```bash
sudo supervisorctl reload
```

To check the status of the app run:

```bash
sudo supervisorctl status
```

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

### Simple CI CD pipeline for .net core web api

In this example we are gonna configure webhook for this project. Everytime I push to master it will execute the following script on the server (`build_and_restart.sh`):

```bash
#!/bin/bash
cd ~/git-webhook-server
git reset --hard HEAD
git pull
supervisorctl stop git-webhook-server
dotnet publish -r linux-x64 -o ~/linux-x64 -c Release
supervisorctl start git-webhook-server
```

**Note:**:
In order to restart it self we need to run `build_and_restart.sh` though a *"proxy"* script. Because when we stop the partner process (in our case `git-webhook-server`) all it's child processes will terminate and two last lines will never be executed.
You do not need this *"proxy"* script if you restarting another application.

That is why we need to run `run_in_tmux.sh` script as a *"proxy"*:

```bash
#!/bin/bash
tmux new -d -s proxy ./build_and_restart.sh
```

**Prerequisites:**

- Git repo is cloned to `~/git-webhook-server`
- Supervisor is installed and configured to run app from: `~/linux-x64` folder
- GitHub WebHook is configured
- `tmux` is installed
- `appsettings.json` has the following rule:

```json
{
    "Name": "master",
    "Ref": "refs/heads/master",
    "RepositoryUrl": "https://github.com/drussilla/git-webhook-server/",
    "Execute": "run_in_tmux.sh"
},
```

- `build_and_restart.sh` script is in the `~/linux-x64` folder
- `run_in_tmux.sh` script is in the `~/linux-x64` folder
- You have permission to run supervisorctl as a regular user (edit `/etc/supervisor/supervisord.conf` and restart `supervisord`):

```ini
[unix_http_server]
file=/var/run/supervisor.sock
chmod=0770
chown=nobody:<username>
```
