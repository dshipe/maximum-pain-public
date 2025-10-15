export class BlogEntry {
    id: number;
    title: string;
    imageUrl: string;
    content: string;
    ordinal: number;
    isActive: boolean;
    isStockPick: boolean;
    showOnHome: boolean;
    createdOn: Date;
    modifiedOn: Date;

    constructor() { }

    get dash(): string {
        return this.title.replace(/\s/g, '-');
    }

    get summary(): string {
        return this.content.substr(30) + "...";
    }
} 
