# Leave Entitlement Configuration Fix

## Problem
The `CalculateEmployeeEntitlementAsync` method in `LeaveService.cs` uses hardcoded values that prevent proper leave management:

```csharp
var baseEntitlement = 20m; // Default to 20 days
var annualIncrement = 1m; // Increment by 1 each year
var maxEntitlement = 30m; // Cap at 30 days
```

## Issues with Current Implementation
1. **30-day cap is too restrictive** - With carryover, employees could have more than 30 days total
2. **Hardcoded values** - You can't control these from the admin interface
3. **Inconsistent with database** - The `EmployeeLeaveEntitlement` table has configurable fields that aren't being used

## Solution
Replace the hardcoded values with configurable values from the `EmployeeLeaveEntitlement` table.

## Manual Fix Required

### Step 1: Locate the Method
Find the `CalculateEmployeeEntitlementAsync` method in `HCMPo/HCMPo/Services/LeaveService.cs` around lines 620-650.

### Step 2: Replace the Annual Leave Calculation Section
**FIND THIS CODE:**
```csharp
if (leaveType.Name.ToLower().Contains("annual"))
{
    var yearsOfService = await CalculateYearsOfServiceAsync(employeeId, new DateTime(year, 12, 31));
    var baseEntitlement = 20m; // Default to 20 days
    var annualIncrement = 1m; // Increment by 1 each year
    var maxEntitlement = 30m; // Cap at 30 days

    var calculatedEntitlement = baseEntitlement + (yearsOfService * annualIncrement);
    return Math.Min(calculatedEntitlement, maxEntitlement);
}
```

**REPLACE WITH:**
```csharp
if (leaveType.Name.ToLower().Contains("annual"))
{
    var yearsOfService = await CalculateYearsOfServiceAsync(employeeId, new DateTime(year, 12, 31));
    
    // Get or create the employee entitlement record to use configurable values
    var entitlement = await GetOrCreateEmployeeEntitlementAsync(employeeId, leaveTypeId);
    
    // Use configurable values from the database instead of hardcoded ones
    var baseEntitlement = entitlement.BaseEntitlement;
    var annualIncrement = entitlement.AnnualIncrement;
    var maxEntitlement = entitlement.MaxEntitlement;

    var calculatedEntitlement = baseEntitlement + (yearsOfService * annualIncrement);
    
    // Only apply max entitlement cap if it's greater than 0 (0 means no cap)
    if (maxEntitlement > 0)
    {
        return Math.Min(calculatedEntitlement, maxEntitlement);
    }
    
    return calculatedEntitlement;
}
```

## Benefits After the Fix

### 1. **Configurable Leave Policies**
- Base entitlement: 20 days (configurable)
- Annual increment: 1 day per year (configurable)
- Max entitlement: 30 days (configurable, 0 = no cap)

### 2. **Proper Carryover Support**
- Employees can now have more than 30 days total when carryover is included
- Example: 25 base + 8 carryover = 33 days (previously capped at 30)

### 3. **Database-Driven Configuration**
- All values are stored in `EmployeeLeaveEntitlement` table
- Can be managed through the admin interface
- Different policies for different employees/leave types

### 4. **Flexible Cap System**
- Set `MaxEntitlement = 0` for no cap
- Set `MaxEntitlement = 40` for a 40-day cap
- Set `MaxEntitlement = 30` for the original 30-day cap

## Database Fields Used
The fix uses these fields from `EmployeeLeaveEntitlement`:
- `BaseEntitlement`: Starting leave days (default: 20)
- `AnnualIncrement`: Days added per year of service (default: 1)
- `MaxEntitlement`: Maximum entitlement cap (default: 30, 0 = no cap)

## Testing After Fix
1. Set custom entitlements for employees
2. Test carryover functionality
3. Verify leave calculations use the correct values
4. Check that employees can have more than 30 days when carryover is included 