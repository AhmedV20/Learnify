# Git Workflow & Branching Strategy

This project follows a rigorous branching strategy to ensure code quality, stability, and a clean history.

## Branching Strategy

We use a strategy inspired by **GitFlow** and **Trunk-Based Development**.

### Main Branches

*   **`main`**: The official release history. This branch is **protected**. It always reflects production-ready code.
*   **`develop`**: The main integration branch. All feature branches are merged into `develop`. This branch serves as the staging area for the next release.

### Temporary Branches

*   **Feature Branches** (`feature/description`): Created from `develop`, merged back into `develop`. Used for developing new features.
*   **Bugfix Branches** (`fix/description`): Created from `develop`, merged back into `develop`. Used for fixing bugs found during development/testing.
*   **Hotfix Branches** (`hotfix/description`): Created from `main`, merged into both `main` and `develop`. Used for critical production bugs.

## Commit Convention

We follow the **Conventional Commits** specification to keep our commit history readable and to enable automated changelogs.

**Format**: `<type>(<scope>): <subject>`

### Types

*   **`feat`**: A new feature
*   **`fix`**: A bug fix
*   **`docs`**: Documentation only changes
*   **`style`**: Changes that do not affect the meaning of the code (white-space, formatting, etc.)
*   **`refactor`**: A code change that neither fixes a bug nor adds a feature
*   **`perf`**: A code change that improves performance
*   **`test`**: Adding missing tests or correcting existing tests
*   **`chore`**: Changes to the build process or auxiliary tools and libraries such as documentation generation

### Example

```bash
git commit -m "feat(auth): implement JWT token expiration"
```

## Pull Request Process

1.  Ensure all tests pass locally.
2.  Update relevant documentation (`README.md`, `swagger` annotations).
3.  Target the `develop` branch for Features and Fixes.
4.  Target the `main` branch ONLY for Hotfixes and Releases.
5.  Link related issues in the PR description.
6.  Request at least one review from a team member.
