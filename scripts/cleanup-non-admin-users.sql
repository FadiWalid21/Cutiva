-- One-off: keep only platform admin + subscription catalog + global materials.
-- Preserves: fadialhallag@gmail.com, AspNetRoles, SubscriptionPlans, Sectors, Materials, StockBars
BEGIN;

DO $$
DECLARE
  admin_id text;
BEGIN
  SELECT "Id" INTO admin_id
  FROM "AspNetUsers"
  WHERE lower("Email") = lower('fadialhallag@gmail.com');

  IF admin_id IS NULL THEN
    RAISE EXCEPTION 'Admin user fadialhallag@gmail.com not found — aborting.';
  END IF;

  -- Workshop-scoped business data (all workshops; admin has no WorkshopId)
  DELETE FROM "ClientQuotes";
  DELETE FROM "CuttingPlanUnallocatedPieces";
  DELETE FROM "CuttingPlanPieces";
  DELETE FROM "CuttingPlanBars";
  DELETE FROM "CuttingPlans";
  DELETE FROM "ProjectSectionAccessories";
  DELETE FROM "RequiredCuts";
  DELETE FROM "ProjectSections";
  DELETE FROM "Projects";
  DELETE FROM "Accessories";
  DELETE FROM "WorkshopSubscriptions";
  DELETE FROM "Workshops";

  -- Identity for everyone except admin
  DELETE FROM "PasswordResetOtps"
  WHERE "UserId" <> admin_id;

  DELETE FROM "AspNetUserTokens"
  WHERE "UserId" <> admin_id;

  DELETE FROM "AspNetUserLogins"
  WHERE "UserId" <> admin_id;

  DELETE FROM "AspNetUserClaims"
  WHERE "UserId" <> admin_id;

  DELETE FROM "AspNetUserRoles"
  WHERE "UserId" <> admin_id;

  DELETE FROM "AspNetUsers"
  WHERE "Id" <> admin_id;

  -- Ensure admin stays unbound from any workshop
  UPDATE "AspNetUsers"
  SET "WorkshopId" = NULL
  WHERE "Id" = admin_id;
END $$;

COMMIT;

-- Preview
SELECT "Email", "WorkshopId", "IsPrimaryContact" FROM "AspNetUsers";
SELECT COUNT(*) AS workshops FROM "Workshops";
SELECT "Code", "Name", "MaxUsers" FROM "SubscriptionPlans" ORDER BY "PriceSar";
