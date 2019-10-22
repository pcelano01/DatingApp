import { Pagination } from './pagination';

export interface PaginatedResult<T> {
    result: T;
    pagination: Pagination;
}
