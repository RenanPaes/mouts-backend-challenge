[Back to README](../README.md)

### Sales

The Sales API handles sales records following DDD and the External Identities pattern:
customer, branch and product references are stored by id together with a denormalized
description. Quantity-based discounts are applied automatically per item.

**Discount rules**
- 4 to 9 identical items: 10% discount
- 10 to 20 identical items: 20% discount
- Below 4 identical items: no discount
- Above 20 identical items: not allowed (returns `400 ValidationError`)

All responses are wrapped in the standard API envelope (`success`, `message`, `data`).
Errors use the `type` / `error` / `detail` format described in the [General API](./general-api.md) section.

#### POST /api/sales
- Description: Create a new sale
- Request Body:
  ```json
  {
    "saleNumber": "string",
    "saleDate": "string (date-time)",
    "customerId": "guid",
    "customerName": "string",
    "branchId": "guid",
    "branchName": "string",
    "items": [
      {
        "productId": "guid",
        "productTitle": "string",
        "quantity": "integer",
        "unitPrice": "number"
      }
    ]
  }
  ```
- Response: `201 Created`
  ```json
  {
    "data": {
      "id": "guid",
      "saleNumber": "string",
      "saleDate": "string (date-time)",
      "customerId": "guid",
      "customerName": "string",
      "branchId": "guid",
      "branchName": "string",
      "totalAmount": "number",
      "isCancelled": "boolean",
      "createdAt": "string (date-time)",
      "updatedAt": "string (date-time) | null",
      "items": [
        {
          "id": "guid",
          "productId": "guid",
          "productTitle": "string",
          "quantity": "integer",
          "unitPrice": "number",
          "discountPercentage": "number",
          "discount": "number",
          "total": "number",
          "isCancelled": "boolean"
        }
      ]
    },
    "success": true,
    "message": "Sale created successfully"
  }
  ```

#### GET /api/sales
- Description: Retrieve a paginated list of sales
- Query Parameters:
  - `_page` (optional): Page number (default: 1)
  - `_size` (optional): Items per page (default: 10)
  - `_order` (optional): Ordering, e.g. `"saleDate desc, saleNumber asc"`
- Response:
  ```json
  {
    "data": [ { "id": "guid", "saleNumber": "string", "totalAmount": "number", "items": [] } ],
    "totalCount": "integer",
    "currentPage": "integer",
    "totalPages": "integer",
    "success": true
  }
  ```

#### GET /api/sales/{id}
- Description: Retrieve a specific sale by id
- Path Parameters:
  - `id`: Sale id (guid)
- Response: `200 OK` (same sale shape as POST) or `404 ResourceNotFound`

#### PUT /api/sales/{id}
- Description: Update a sale, replacing its header data and items (totals are recalculated)
- Path Parameters:
  - `id`: Sale id (guid)
- Request Body: same shape as `POST /api/sales`
- Response: `200 OK` (updated sale) — raises the `SaleModified` event

#### DELETE /api/sales/{id}
- Description: Delete a sale
- Path Parameters:
  - `id`: Sale id (guid)
- Response:
  ```json
  { "success": true, "message": "Sale deleted successfully" }
  ```

#### POST /api/sales/{id}/cancel
- Description: Cancel an entire sale and all of its items (total becomes 0)
- Path Parameters:
  - `id`: Sale id (guid)
- Response: `200 OK` (cancelled sale) — raises the `SaleCancelled` event

#### POST /api/sales/{saleId}/items/{itemId}/cancel
- Description: Cancel a single item within a sale (total is recalculated)
- Path Parameters:
  - `saleId`: Sale id (guid)
  - `itemId`: Item id (guid)
- Response: `200 OK` (updated sale) — raises the `ItemCancelled` event

### Events

The following domain events are published (via MediatR) and written to the application log:
`SaleCreated`, `SaleModified`, `SaleCancelled`, `ItemCancelled`.

<br>
<div style="display: flex; justify-content: space-between;">
  <a href="./carts-api.md">Previous: Carts API</a>
  <a href="./general-api.md">Next: General API</a>
</div>
