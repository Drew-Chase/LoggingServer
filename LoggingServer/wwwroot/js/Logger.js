/**
 * Represents the available types of log messages.
 * @enum {string}
 * @readonly
 */
const LogTypes = {
    DEBUG: 'DEBUG',
    INFO: 'INFO',
    ERROR: 'ERROR',
    WARNING: 'WARNING',
    FATAL: 'FATAL',
};

/**
 * Logger class for logging messages and sending them to a log server.
 */
export class Logger {


    /**
     * @param {string} server - The server url or ipaddress to connect to
     */
    constructor(server) {
        this.server = server;
    }

    /**
     * Logs a debug message and sends it to the log server.
     *
     * @param {string} message - The debug message to log.
     * @return {Promise<void>} A Promise that resolves when the message is logged and sent to the server.
     */
    async debug(message) {
        console.debug(message);
        await this.sendLog(message, LogTypes.DEBUG);
    }

    /**
     * Logs the provided message with the INFO log type.
     *
     * @param {string} message - The message to log.
     *
     * @return {Promise<void>} - A promise that resolves when the log is sent.
     */
    async info(message) {
        console.log(message);
        await this.sendLog(message, LogTypes.INFO);
    }

    /**
     * Logs a warning message and sends it to the server.
     *
     * @param {string} message - The warning message.
     * @return {Promise} - A promise that resolves when the warning message is logged and sent.
     */
    async warning(message) {
        console.warn(message);
        await this.sendLog(message, LogTypes.WARNING);
    }

    /**
     * Logs an error message and sends it to the server.
     *
     * @param {string} message - The error message to be logged.
     *
     * @return {Promise<void>} - A promise that resolves when the error message has been sent to the server.
     */
    async error(message) {
        console.error(message);
        await this.sendLog(message, LogTypes.ERROR);
    }


    /**
     * Logs a fatal error message and sends it to the log server asynchronously.
     *
     * @param {string} message - The fatal error message to log.
     * @return {Promise<void>} - A promise that resolves when the log is sent.
     */
    async fatal(message) {
        console.error(message);
        await this.sendLog(message, LogTypes.FATAL);
    }

    /**
     * Sends a log message to the server
     * @param {string} message - The message to log
     * @param {LogTypes} type - The type of log message
     */
    async sendLog(message, type) {
        await fetch(`${this.server}/${window.location.hostname}/${type}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({message: message})
        });
    }
}
