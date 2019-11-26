const [, , file] = process.argv;
const config = require(file);
process.stdout.write(config.output.path);
