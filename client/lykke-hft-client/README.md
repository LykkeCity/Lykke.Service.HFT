# An isomorphic javascript sdk for - Lykke High-Frequency Trading (HFT) API
This project provides an isomorphic javascript package. Right now it supports:
- node.js version 6.x.x or higher
- browser javascript

## How to Install

- nodejs
```
npm install lykke-hft-client
```
- browser
```html
<script type="text/javascript" src="lykke-hft-client/highFrequencytradingAPIBundle.js"></script>
```

## How to use

### nodejs - getAssetPairs as an example written in TypeScript.

```javascript
import * as msRest from "ms-rest-js";
import { HighFrequencytradingAPI, HighFrequencytradingAPIModels, HighFrequencytradingAPIMappers } from "lykke-hft-client";

const client = new HighFrequencytradingAPI("https://hft-api.lykke.com");
client.getAssetPairs().then((result) => {
  console.log("The result is:");
  console.log(result);
}).catch((err) => {
  console.log('An error ocurred:');
  console.dir(err, {depth: null, colors: true});
});
```

### browser - getAssetPairs  as an example written in javascript.

- index.html
```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <title>My Todos</title>
    <script type="text/javascript" src="https://raw.githubusercontent.com/Azure/ms-rest-js/master/msRestBundle.js"></script>
    <script type="text/javascript" src="./highFrequencytradingAPIBundle.js"></script>
    <script type="text/javascript">
      document.write('hello world');
      const client = new HighFrequencytradingAPI("https://hft-api.lykke.com");
      client.getAssetPairs().then((result) => {
        console.log("The result is:");
        console.log(result);
      }).catch((err) => {
        console.log('An error ocurred:');
        console.dir(err, { depth: null, colors: true});
      });
    </script>
  </head>
  <body>
  </body>
</html>
```

# Related projects
 - [Lykke Swagger Documentation](https://hft-api.lykke.com/swagger/ui/)
 - [Lykke HFT Service](https://github.com/LykkeCity/Lykke.Service.HFT)
 - [Microsoft Azure SDK for Javascript](https://github.com/Azure/azure-sdk-for-js)
