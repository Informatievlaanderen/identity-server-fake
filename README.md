# identity-server-fake
Identity Server implementation which can be configured by json files.

## What is this for
This docker image will allow you to replace the ACM/IDM system during some of your testing activities, 
both locally as well as part of your CI build workflow. 

## Required tools
- .net sdk
- docker-compose

## Based on
- DuendeSoftware [IdentityServer.Templates](https://github.com/DuendeSoftware/IdentityServer.Templates)

## Usage
The identity server will load json config files from the configuration folder `/home/identityserver`.

You will want to provide your own config files by mapping `/home/identityserver` to the folder containing your config files. 
A config file should contain the json representation of the configuration as needed by the IdentityServer.

You can try out this repository's example configuration file by mapping the configuration folder to the `example-config` folder found at the root of this repository, see the example below.
 
### Example docker compose files
#### Providing your own config files
```
version: '3'
services:
  identity-server-image:
     image: ghcr.io/informatievlaanderen/identity-server-fake:latest
     volumes:
       - ./test/IdentityServer.Test/config:/home/identityserver
     ports:
       - "5050:80"
```

#### Using the example config
```
version: '3'
services:
  identity-server-image:
     image: ghcr.io/informatievlaanderen/identity-server-fake:latest
     volumes:
       - ./example-config:/home/identityserver
     ports:
       - "5050:80"
```

### Example json configuration
You can find example config files [here](example-config).

## How to build for local development
- Install the [required](global.json) .NET Core SDK
- Run `docker compose up --build` in the root
- Run `dotnet build` in the root
- Run `dotnet test` in the root