+++
title = "Hosting Blazor"
weight = 40
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-03-22
+++

{{% notice note %}}
{{% siteparam "disclaimer" %}}
{{% /notice %}}

## Introduction

Hosting a Blazor app should be trivial, shouldn' it? It is just a bunch of static files (e.g. HTML, JavaScript, DLLs, etc.) so we can put it on any static web server and we are done. This idea is not wrong. If you try it, basic tests will succeed. However, you will recognize problems with routing once you look closer.

## The Problem

Let's take a look at an example. Imagine a Blazor app consisting of two pages, *Page1.cshtml* and *Page2.cshtml*. Let's assume that it is hosted on a simple static web server (e.g. [nginx](https://nginx.org/en/)). Personally, I like to use [*Docker* containers](https://hub.docker.com/_/nginx/) for that. A simple demo *Dockerfile* for statically hosting Blazor in *nginx* could look like this:

```Dockerfile
FROM nginx:alpine
COPY ./bin/Debug/netstandard2.0/dist /usr/share/nginx/html/
```

Let's assume that our *nginx* server listens on *http://localhost:8082/*. If you open this URL, you will see your default route and all the router links will work as expected. However, if you enter *http://localhost:8082/Page1* manually in your browser's address bar, you will get a 404 *not found* error.

The reason is the difference between client- and server-side routing. If you load your Blazor app without a route, the webserver will send your *index.html* page to the browser client. It contains Blazor's JavaScript and everything is fine. The JavaScript code handles routing on the client-side. If you try to navigate directly to a route in your Blazor app, the URL (e.g. *http://localhost:8082/Page1*) is sent to the *server* and it does not know what *Page1* means. Therefore, you see a 404 error.

## The Solution

We have to configure our static web server to always deliver *index.html* if it receives a URL that will be handled by Blazor's router on the client. Once *index.html* is on the client, it's referenced JavaScript/C# code will care for proper processing of the route.

The *nginx* server mentioned above allows to define such rules in its config files. Here is a simplified example for an *nginx.conf* file that sends *index.html* whenever it cannot find a corresponding file on disk. 

{{% notice note %}}
Note that this config file is very much simplified to demonstrate the concept. For a production web server, your config file would probably look quite different ([read more about nginx config files](https://docs.nginx.com/nginx/admin-guide/basic-functionality/managing-configuration-files/)).
{{% /notice %}}

```config
events { }
http {
    server {
        listen 80;

        location / {
            root /usr/share/nginx/html;
            try_files $uri $uri/ /index.html =404;
        }
    }
}
```

If you want to try *nginx* in Docker, add one line to your Dockerfile and try your Blazor app. Routes like *http://localhost:8082/Page1* will now work.

```Dockerfile
FROM nginx:alpine
COPY ./bin/Debug/netstandard2.0/dist /usr/share/nginx/html/
COPY nginx.conf /etc/nginx/nginx.conf
```

If you use different webservers, the configuration settings will be different but the general concept is the same. The [*Webserver for Chrome*](https://chrome.google.com/webstore/detail/web-server-for-chrome/ofhbbkphhbklhfoeikjpcbhemlocgigb/related?utm_source=chrome-app-launcher-info-dialog) for instance offers rewrite options for Single Page Apps in its advanced options:

![Settings Webserver for Chrome](/images/getting-started/webserver-for-chrome-settings.png)

## GitHub Pages

[GitHub Pages](https://pages.github.com/) can also be used to host Single Page Apps like Blazor. You can find a description of the necessary steps to make routing work e.g. [on this website](http://spa-github-pages.rafrex.com/).
