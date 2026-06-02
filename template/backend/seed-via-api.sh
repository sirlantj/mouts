#!/bin/bash
set -e

API="${API:-http://localhost:8080/api}"
EMAIL="admin@ambev.com"
PASSWORD="Test@123"

echo ">>> Limpando Sales/SaleItems no Postgres..."
docker exec ambev_developer_evaluation_database psql -U developer -d developer_evaluation -c \
  'TRUNCATE TABLE "SaleItems", "Sales" CASCADE;' > /dev/null

echo ">>> Limpando collection sales_read no MongoDB..."
docker exec ambev_developer_evaluation_nosql mongosh \
  -u developer -p 'ev@luAt10n' --authenticationDatabase admin \
  --quiet --eval 'db.getSiblingDB("developer_evaluation").sales_read.deleteMany({}); db.getSiblingDB("developer_evaluation").sale_events.deleteMany({})' > /dev/null

echo ">>> Login..."
TOKEN=$(curl -sS -X POST "$API/auth" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}" \
  | python3 -c "import json,sys; print(json.load(sys.stdin)['data']['data']['token'])")
echo "    Token obtido: ${TOKEN:0:30}..."

create_sale() {
  local payload="$1"
  local label="$2"
  local response
  response=$(curl -sS -X POST "$API/sales" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d "$payload")
  local id
  id=$(echo "$response" | python3 -c "
import json,sys
d=json.load(sys.stdin)
inner=d.get('data',{})
# Some endpoints return data.id; others wrap as data.data.id
if isinstance(inner, dict) and 'id' in inner:
    print(inner['id'])
elif isinstance(inner, dict) and isinstance(inner.get('data'), dict):
    print(inner['data'].get('id',''))
" 2>/dev/null)
  if [ -z "$id" ]; then
    echo "    [$label] FAILED - response: $response" >&2
  else
    echo "    [$label] id=$id" >&2
  fi
  echo "$id"
}

echo ">>> Criando vendas..."

# SALE-001: Sem desconto (qty < 4)
ID1=$(create_sale '{
  "saleNumber":"SALE-001",
  "saleDate":"2025-06-01T14:30:00Z",
  "customerExternalId":"CUST-001",
  "customerName":"Distribuidora Silva",
  "branchExternalId":"BR-SP-001",
  "branchName":"Filial Sao Paulo Centro",
  "items":[
    {"productExternalId":"PROD-001","productName":"Brahma Lata 350ml","quantity":2,"unitPrice":15.00},
    {"productExternalId":"PROD-002","productName":"Skol Lata 350ml","quantity":3,"unitPrice":15.00}
  ]}' "SALE-001 (sem desconto)")

# SALE-002: 10%
ID2=$(create_sale '{
  "saleNumber":"SALE-002",
  "saleDate":"2025-05-28T09:15:00Z",
  "customerExternalId":"CUST-002",
  "customerName":"Bar do Joao",
  "branchExternalId":"BR-RJ-001",
  "branchName":"Filial Rio de Janeiro",
  "items":[
    {"productExternalId":"PROD-003","productName":"Budweiser Long Neck 330ml","quantity":6,"unitPrice":40.00}
  ]}' "SALE-002 (10% desconto)")

# SALE-003: 20%
ID3=$(create_sale '{
  "saleNumber":"SALE-003",
  "saleDate":"2025-05-20T16:45:00Z",
  "customerExternalId":"CUST-003",
  "customerName":"Restaurante Mineiro",
  "branchExternalId":"BR-MG-001",
  "branchName":"Filial Belo Horizonte",
  "items":[
    {"productExternalId":"PROD-004","productName":"Antarctica Original 600ml","quantity":12,"unitPrice":40.00}
  ]}' "SALE-003 (20% desconto)")

# SALE-004: Mix
ID4=$(create_sale '{
  "saleNumber":"SALE-004",
  "saleDate":"2025-05-15T11:00:00Z",
  "customerExternalId":"CUST-001",
  "customerName":"Distribuidora Silva",
  "branchExternalId":"BR-SP-001",
  "branchName":"Filial Sao Paulo Centro",
  "items":[
    {"productExternalId":"PROD-005","productName":"Guarana Antarctica 2L","quantity":2,"unitPrice":25.00},
    {"productExternalId":"PROD-006","productName":"Stella Artois Lata 350ml","quantity":5,"unitPrice":30.00},
    {"productExternalId":"PROD-007","productName":"Corona Extra Long Neck 330ml","quantity":15,"unitPrice":38.33}
  ]}' "SALE-004 (mix de descontos)")

# SALE-005: Sera cancelada apos criacao
ID5=$(create_sale '{
  "saleNumber":"SALE-005",
  "saleDate":"2025-04-10T08:00:00Z",
  "customerExternalId":"CUST-004",
  "customerName":"Padaria Central",
  "branchExternalId":"BR-SP-002",
  "branchName":"Filial Sao Paulo Zona Sul",
  "items":[
    {"productExternalId":"PROD-001","productName":"Brahma Lata 350ml","quantity":8,"unitPrice":15.00}
  ]}' "SALE-005 (sera cancelada)")

# SALE-006: Item sera cancelado parcialmente
ID6=$(create_sale '{
  "saleNumber":"SALE-006",
  "saleDate":"2025-05-25T13:20:00Z",
  "customerExternalId":"CUST-002",
  "customerName":"Bar do Joao",
  "branchExternalId":"BR-RJ-001",
  "branchName":"Filial Rio de Janeiro",
  "items":[
    {"productExternalId":"PROD-008","productName":"Pepsi Lata 350ml","quantity":3,"unitPrice":20.00},
    {"productExternalId":"PROD-009","productName":"Bohemia Lata 350ml","quantity":4,"unitPrice":25.00}
  ]}' "SALE-006 (item sera cancelado)")

# SALE-007: Filial Salvador
ID7=$(create_sale '{
  "saleNumber":"SALE-007",
  "saleDate":"2025-05-10T17:30:00Z",
  "customerExternalId":"CUST-005",
  "customerName":"Mercadinho Nordeste",
  "branchExternalId":"BR-BA-001",
  "branchName":"Filial Salvador",
  "items":[
    {"productExternalId":"PROD-010","productName":"Spaten Puro Malte 600ml","quantity":10,"unitPrice":40.00}
  ]}' "SALE-007 (filial Salvador)")

# SALE-008: Valor alto
ID8=$(create_sale '{
  "saleNumber":"SALE-008",
  "saleDate":"2025-05-05T10:00:00Z",
  "customerExternalId":"CUST-006",
  "customerName":"Atacadao Bebidas LTDA",
  "branchExternalId":"BR-PR-001",
  "branchName":"Filial Curitiba",
  "items":[
    {"productExternalId":"PROD-011","productName":"Whisky Old Parr 750ml","quantity":20,"unitPrice":150.00},
    {"productExternalId":"PROD-012","productName":"Whisky Johnnie Walker Black 750ml","quantity":20,"unitPrice":200.00}
  ]}' "SALE-008 (valor alto)")

# SALE-009: Qty no limite (20)
ID9=$(create_sale '{
  "saleNumber":"SALE-009",
  "saleDate":"2025-05-18T15:00:00Z",
  "customerExternalId":"CUST-003",
  "customerName":"Restaurante Mineiro",
  "branchExternalId":"BR-MG-001",
  "branchName":"Filial Belo Horizonte",
  "items":[
    {"productExternalId":"PROD-013","productName":"Wals Trippel 600ml","quantity":20,"unitPrice":30.00}
  ]}' "SALE-009 (qty 20)")

# SALE-010: Hoje
TODAY=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
ID10=$(create_sale "{
  \"saleNumber\":\"SALE-010\",
  \"saleDate\":\"$TODAY\",
  \"customerExternalId\":\"CUST-001\",
  \"customerName\":\"Distribuidora Silva\",
  \"branchExternalId\":\"BR-SP-001\",
  \"branchName\":\"Filial Sao Paulo Centro\",
  \"items\":[
    {\"productExternalId\":\"PROD-014\",\"productName\":\"Cerveja Patagonia IPA 355ml\",\"quantity\":1,\"unitPrice\":45.00}
  ]}" "SALE-010 (hoje)")

echo ""
echo ">>> Cancelando SALE-005 (DELETE)..."
curl -sS -X DELETE -H "Authorization: Bearer $TOKEN" "$API/sales/$ID5" | python3 -c "import json,sys; d=json.load(sys.stdin); print('    success=', d.get('success'), 'msg=', d.get('message'))"

echo ""
echo ">>> Cancelando o segundo item da SALE-006 (PATCH)..."
SALE6_DETAIL=$(curl -sS -H "Authorization: Bearer $TOKEN" "$API/sales/$ID6")
ITEM_ID=$(echo "$SALE6_DETAIL" | python3 -c "
import json,sys
d=json.load(sys.stdin)
inner=d['data']
items=inner.get('items') or inner['data']['items']
target=[i for i in items if i['productExternalId']=='PROD-009'][0]
print(target['id'])")
echo "    Item PROD-009 id=$ITEM_ID"
curl -sS -X PATCH -H "Authorization: Bearer $TOKEN" "$API/sales/$ID6/items/$ITEM_ID/cancel" | python3 -c "import json,sys; d=json.load(sys.stdin); print('    success=', d.get('success'), 'msg=', d.get('message'))"

echo ""
echo ">>> Resumo Postgres:"
docker exec ambev_developer_evaluation_database psql -U developer -d developer_evaluation -c '
SELECT "Status", COUNT(*) FROM "Sales" GROUP BY "Status";
SELECT "IsCancelled", COUNT(*) FROM "SaleItems" GROUP BY "IsCancelled";'

echo ""
echo ">>> Resumo MongoDB:"
docker exec ambev_developer_evaluation_nosql mongosh \
  -u developer -p 'ev@luAt10n' --authenticationDatabase admin \
  --quiet --eval '
const db2 = db.getSiblingDB("developer_evaluation");
print("sales_read count:", db2.sales_read.countDocuments());
print("sale_events count:", db2.sale_events.countDocuments());'

echo ""
echo "Done!"
