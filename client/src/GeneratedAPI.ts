//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v14.0.4.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

/* tslint:disable */
/* eslint-disable */
// ReSharper disable InconsistentNaming

export class GeneratedAPI {
    private http: { fetch(url: RequestInfo, init?: RequestInit): Promise<Response> };
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(baseUrl?: string, http?: { fetch(url: RequestInfo, init?: RequestInit): Promise<Response> }) {
        this.http = http ? http : window as any;
        this.baseUrl = baseUrl ?? "";
    }

    /**
     * @return Success
     */
    tryGetSavedGame(): Promise<SavedGameResult> {
        let url_ = this.baseUrl + "/api/v1/game/tryGetSavedGame";
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "POST",
            headers: {
                "Accept": "text/plain"
            }
        };

        return this.http.fetch(url_, options_).then((_response: Response) => {
            return this.processTryGetSavedGame(_response);
        });
    }

    protected processTryGetSavedGame(response: Response): Promise<SavedGameResult> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            result200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver) as SavedGameResult;
            return result200;
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<SavedGameResult>(null as any);
    }

    /**
     * @param isWhite (optional) 
     * @param isTwoPlayer (optional) 
     * @return Success
     */
    startGame(isWhite: boolean | undefined, isTwoPlayer: boolean | undefined): Promise<BoardDisplay> {
        let url_ = this.baseUrl + "/api/v1/game/startGame?";
        if (isWhite === null)
            throw new Error("The parameter 'isWhite' cannot be null.");
        else if (isWhite !== undefined)
            url_ += "isWhite=" + encodeURIComponent("" + isWhite) + "&";
        if (isTwoPlayer === null)
            throw new Error("The parameter 'isTwoPlayer' cannot be null.");
        else if (isTwoPlayer !== undefined)
            url_ += "isTwoPlayer=" + encodeURIComponent("" + isTwoPlayer) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "POST",
            headers: {
                "Accept": "text/plain"
            }
        };

        return this.http.fetch(url_, options_).then((_response: Response) => {
            return this.processStartGame(_response);
        });
    }

    protected processStartGame(response: Response): Promise<BoardDisplay> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            result200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver) as BoardDisplay;
            return result200;
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<BoardDisplay>(null as any);
    }

    /**
     * @return Success
     */
    compMove(): Promise<BoardDisplay> {
        let url_ = this.baseUrl + "/api/v1/game/compMove";
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "POST",
            headers: {
                "Accept": "text/plain"
            }
        };

        return this.http.fetch(url_, options_).then((_response: Response) => {
            return this.processCompMove(_response);
        });
    }

    protected processCompMove(response: Response): Promise<BoardDisplay> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            result200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver) as BoardDisplay;
            return result200;
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<BoardDisplay>(null as any);
    }

    /**
     * @param row (optional) 
     * @param col (optional) 
     * @return Success
     */
    click(row: number | undefined, col: number | undefined): Promise<ClickReturn> {
        let url_ = this.baseUrl + "/api/v1/game/click?";
        if (row === null)
            throw new Error("The parameter 'row' cannot be null.");
        else if (row !== undefined)
            url_ += "row=" + encodeURIComponent("" + row) + "&";
        if (col === null)
            throw new Error("The parameter 'col' cannot be null.");
        else if (col !== undefined)
            url_ += "col=" + encodeURIComponent("" + col) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "POST",
            headers: {
                "Accept": "text/plain"
            }
        };

        return this.http.fetch(url_, options_).then((_response: Response) => {
            return this.processClick(_response);
        });
    }

    protected processClick(response: Response): Promise<ClickReturn> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            result200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver) as ClickReturn;
            return result200;
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<ClickReturn>(null as any);
    }

    /**
     * @return Success
     */
    sendEmail(): Promise<boolean> {
        let url_ = this.baseUrl + "/api/v1/communication/sendEmail";
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "POST",
            headers: {
                "Accept": "text/plain"
            }
        };

        return this.http.fetch(url_, options_).then((_response: Response) => {
            return this.processSendEmail(_response);
        });
    }

    protected processSendEmail(response: Response): Promise<boolean> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            result200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver) as boolean;
            return result200;
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<boolean>(null as any);
    }
}

export interface BoardDisplay {
    rows?: BoardDisplayRow[] | undefined;
}

export interface BoardDisplayRow {
    squares?: BoardDisplaySquare[] | undefined;
}

export interface BoardDisplaySquare {
    col?: number;
    row?: number;
    cssClass?: string | undefined;
}

export interface ClickReturn {
    board: BoardDisplay;
    moved: boolean;
}

export interface SavedGameResult {
    boardDisplay?: BoardDisplay;
    isPlayerWhite?: boolean;
    isTwoPlayer?: boolean;
}

export class ApiException extends Error {
    message: string;
    status: number;
    response: string;
    headers: { [key: string]: any; };
    result: any;

    constructor(message: string, status: number, response: string, headers: { [key: string]: any; }, result: any) {
        super();

        this.message = message;
        this.status = status;
        this.response = response;
        this.headers = headers;
        this.result = result;
    }

    protected isApiException = true;

    static isApiException(obj: any): obj is ApiException {
        return obj.isApiException === true;
    }
}

function throwException(message: string, status: number, response: string, headers: { [key: string]: any; }, result?: any): any {
    if (result !== null && result !== undefined)
        throw result;
    else
        throw new ApiException(message, status, response, headers, null);
}