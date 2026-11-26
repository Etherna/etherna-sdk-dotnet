# Use Nswag

Use Nswag to generate client code. Use NSwagStudio for Windows, or NSwag Cli with others.

NSwag Cli:

```shell
nswag run etherna-credit-api.nswag
nswag run etherna-gateway-api.nswag
nswag run etherna-index-api.nswag
nswag run etherna-sso-api.nswag
```

After regenerating, re-apply the Native AOT fixes of the generated clients documented in
[doc/NativeAot.md](../doc/NativeAot.md): the build fails until they are.